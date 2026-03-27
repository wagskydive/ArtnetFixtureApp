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

      // 🔐 STEP 1 — Validate incoming purchase with Google
      const validation = await validateWithGoogle(env, productId, purchaseToken);

      // 🔐 STEP 2 — Load stored tokens for device
      const storageKey = `device:${deviceId}`;
      let stored = await env.PURCHASES.get(storageKey, "json");
      if (!stored) stored = [];

      // 🔐 STEP 3 — Prevent token reuse across devices
      const globalTokenKey = `token:${purchaseToken}`;
      const existingOwner = await env.PURCHASES.get(globalTokenKey);

      if (existingOwner && existingOwner !== deviceId) {
        return json({
          productId,
          valid: false,
          revoked: true
        });
      }

      // 🔐 STEP 4 — Store token if valid
      if (validation.valid) {
        const alreadyStored = stored.find(e => e.token === purchaseToken);

        if (!alreadyStored) {
          stored.push({ productId, token: purchaseToken });

          await env.PURCHASES.put(storageKey, JSON.stringify(stored));
          await env.PURCHASES.put(globalTokenKey, deviceId);
        }
      }

      // 🔐 STEP 5 — Revalidate ALL stored tokens
      let validProducts = [];
      let revokedProducts = [];

      for (const entry of stored) {
        const result = await validateWithGoogle(env, entry.productId, entry.token);

        if (result.valid) {
          validProducts.push(entry.productId);
        } else {
          revokedProducts.push(entry.productId);
        }
      }

      // 🔐 STEP 6 — Clean revoked tokens
      const cleaned = stored.filter(entry =>
        validProducts.includes(entry.productId)
      );

      await env.PURCHASES.put(storageKey, JSON.stringify(cleaned));

      // 🔐 STEP 7 — Response
      return json({
        productId,
        valid: validProducts.includes(productId),
        revoked: revokedProducts.includes(productId)
      });

    } catch (e) {
      return json({ valid: false });
    }
  }
};

// =========================
// 🔐 GOOGLE VALIDATION
// =========================
async function validateWithGoogle(env, productId, purchaseToken) {
  const cacheKey = `cache:${purchaseToken}`;
  const cachedRaw = await env.PURCHASE_CACHE.get(cacheKey);

  if (cachedRaw) {
    return JSON.parse(cachedRaw);
  }

  const accessToken = await getAccessToken(env);

  const url = `https://androidpublisher.googleapis.com/androidpublisher/v3/applications/${env.PACKAGE_NAME}/purchases/products/${productId}/tokens/${purchaseToken}`;

  const res = await fetch(url, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  });

  if (!res.ok) {
    return { valid: false };
  }

  const data = await res.json();

  const valid = data.purchaseState === 0;

  const result = {
    valid,
    revoked: !valid
  };

  // cache for 6 hours
  await env.PURCHASE_CACHE.put(
    cacheKey,
    JSON.stringify(result),
    { expirationTtl: 21600 }
  );

  return result;
}

// =========================
// 🔐 AUTH
// =========================
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

// =========================
// 🔐 JWT SIGNING
// =========================
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