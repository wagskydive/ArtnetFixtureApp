export default {
  async fetch(request, env) {
    if (request.method !== "POST") {
      return json({ error: "Method not allowed" }, 405);
    }

    try {
      const body = await request.json();
      const { productId, purchaseToken, deviceId } = body;

      if (!productId || !purchaseToken || !deviceId) {
        return json({ valid: false });
      }

      const cacheKey = `purchase:${purchaseToken}`;
      const cachedRaw = await env.PURCHASE_CACHE.get(cacheKey);

      if (cachedRaw) {
        const cached = JSON.parse(cachedRaw);

        if (cached.deviceId && cached.deviceId !== deviceId) {
          return json({
            productId,
            valid: false,
            revoked: true
          });
        }

        return json(cached);
      }

      const accessToken = await getAccessToken(env);

      const url = `https://androidpublisher.googleapis.com/androidpublisher/v3/applications/${env.PACKAGE_NAME}/purchases/products/${productId}/tokens/${purchaseToken}`;

      const googleRes = await fetch(url, {
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (!googleRes.ok) {
        return json({ productId, valid: false });
      }

      const data = await googleRes.json();

      const isValid = data.purchaseState === 0;

      const response = {
        productId,
        valid: isValid,
        revoked: !isValid,
        deviceId
      };

      await env.PURCHASE_CACHE.put(
        cacheKey,
        JSON.stringify(response),
        { expirationTtl: 86400 }
      );

      return json(response);

    } catch (e) {
      return json({ valid: false });
    }
  }
};

async function getAccessToken(env) {
  const now = Math.floor(Date.now() / 1000);

  const header = { alg: "RS256", typ: "JWT" };

  const payload = {
    iss: env.GOOGLE_CLIENT_EMAIL,
    scope: "https://www.googleapis.com/auth/androidpublisher",
    aud: "https://oauth2.googleapis.com/token",
    exp: now + 3600,
    iat: now
  };

  const jwt = await signJWT(header, payload, env.GOOGLE_PRIVATE_KEY);

  const res = await fetch("https://oauth2.googleapis.com/token", {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: new URLSearchParams({
      grant_type: "urn:ietf:params:oauth:grant-type:jwt-bearer",
      assertion: jwt
    })
  });

  const data = await res.json();
  return data.access_token;
}

async function signJWT(header, payload, privateKeyPem) {
  const enc = (obj) =>
    btoa(JSON.stringify(obj))
      .replace(/\+/g, "-")
      .replace(/\//g, "_")
      .replace(/=+$/, "");

  const data = `${enc(header)}.${enc(payload)}`;

  const key = await crypto.subtle.importKey(
    "pkcs8",
    pemToArrayBuffer(privateKeyPem),
    { name: "RSASSA-PKCS1-v1_5", hash: "SHA-256" },
    false,
    ["sign"]
  );

  const signature = await crypto.subtle.sign(
    "RSASSA-PKCS1-v1_5",
    key,
    new TextEncoder().encode(data)
  );

  const sig = btoa(String.fromCharCode(...new Uint8Array(signature)))
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "");

  return `${data}.${sig}`;
}

function pemToArrayBuffer(pem) {
  const base64 = pem
    .replace("-----BEGIN PRIVATE KEY-----", "")
    .replace("-----END PRIVATE KEY-----", "")
    .replace(/\n/g, "");

  const binary = atob(base64);
  const buffer = new ArrayBuffer(binary.length);
  const view = new Uint8Array(buffer);

  for (let i = 0; i < binary.length; i++) {
    view[i] = binary.charCodeAt(i);
  }

  return buffer;
}

function json(obj, status = 200) {
  return new Response(JSON.stringify(obj), {
    status,
    headers: { "Content-Type": "application/json" }
  });
}