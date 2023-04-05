const { chromium } = require('@playwright/test');

(async () => {
  // Make sure to run headed.
  const browser = await chromium.launch({ headless: false });

  // Setup context however you like.
  const context = await browser.newContext({ /* pass any options */ });
  await context.route('https://login.microsoftonline.com/common/discovery/**/*', async route => {
    const response = {
      "tenant_discovery_endpoint": "https://login.microsoftonline.com/3e232bbb-c7c7-462b-a928-2bd6b5d64c0d/v2.0/.well-known/openid-configuration",
      "api-version": "1.1",
      "metadata": [
        {
          "preferred_network": "login.microsoftonline.com",
          "preferred_cache": "login.windows.net",
          "aliases": [
            "login.microsoftonline.com",
            "login.windows.net",
            "login.microsoft.com",
            "sts.windows.net"
          ]
        }
      ]
    }
    await route.fulfill({ json: response });
  })

  await context.route('https://login.microsoftonline.com/**/openid-configuration', async route => {
    const response = {
      "token_endpoint": "https://login.microsoftonline.com/3e232bbb-c7c7-462b-a928-2bd6b5d64c0d/oauth2/v2.0/token",
      "token_endpoint_auth_methods_supported": [
        "client_secret_post",
        "private_key_jwt",
        "client_secret_basic"
      ],
      "jwks_uri": "https://login.microsoftonline.com/3e232bbb-c7c7-462b-a928-2bd6b5d64c0d/discovery/v2.0/keys",
      "response_modes_supported": [
        "query",
        "fragment",
        "form_post"
      ],
      "subject_types_supported": [
        "pairwise"
      ],
      "id_token_signing_alg_values_supported": [
        "RS256"
      ],
      "response_types_supported": [
        "code",
        "id_token",
        "code id_token",
        "id_token token"
      ],
      "scopes_supported": [
        "openid",
        "profile",
        "email",
        "offline_access"
      ],
      "issuer": "https://login.microsoftonline.com/3e232bbb-c7c7-462b-a928-2bd6b5d64c0d/v2.0",
      "request_uri_parameter_supported": false,
      "userinfo_endpoint": "https://graph.microsoft.com/oidc/userinfo",
      "authorization_endpoint": "https://login.microsoftonline.com/3e232bbb-c7c7-462b-a928-2bd6b5d64c0d/oauth2/v2.0/authorize",
      "device_authorization_endpoint": "https://login.microsoftonline.com/3e232bbb-c7c7-462b-a928-2bd6b5d64c0d/oauth2/v2.0/devicecode",
      "http_logout_supported": true,
      "frontchannel_logout_supported": true,
      "end_session_endpoint": "https://login.microsoftonline.com/3e232bbb-c7c7-462b-a928-2bd6b5d64c0d/oauth2/v2.0/logout",
      "claims_supported": [
        "sub",
        "iss",
        "cloud_instance_name",
        "cloud_instance_host_name",
        "cloud_graph_host_name",
        "msgraph_host",
        "aud",
        "exp",
        "iat",
        "auth_time",
        "acr",
        "nonce",
        "preferred_username",
        "name",
        "tid",
        "ver",
        "at_hash",
        "c_hash",
        "email"
      ],
      "kerberos_endpoint": "https://login.microsoftonline.com/3e232bbb-c7c7-462b-a928-2bd6b5d64c0d/kerberos",
      "tenant_region_scope": "NA",
      "cloud_instance_name": "microsoftonline.com",
      "cloud_graph_host_name": "graph.windows.net",
      "msgraph_host": "graph.microsoft.com",
      "rbac_url": "https://pas.windows.net"
    }
    await route.fulfill({ json: response });
  })

  await context.route('https://login.microsoftonline.com/**/token', async route => {
    const response = {
      "token_type": "Bearer",
      "scope": "openid profile email",
      "expires_in": 4384,
      "ext_expires_in": 4384,
      "access_token": "redacted",
      "refresh_token": "redacted",
      "id_token": "redacted",
      "client_info": "redacted"
    }
    await route.fulfill({ json: response });
  })
  // Pause the page, and start recording manually.
  const page = await context.newPage();
  await page.pause();
})();
