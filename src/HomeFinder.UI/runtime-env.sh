#!/bin/sh
cat > /usr/share/nginx/html/env.js <<EOF
window.__APP_CONFIG__ = {
  VITE_API_BASE_URL: "${VITE_API_BASE_URL:-http://localhost:5000}",
  VITE_AZURE_CLIENT_ID: "${VITE_AZURE_CLIENT_ID:-}",
  VITE_AZURE_TENANT_ID: "${VITE_AZURE_TENANT_ID:-}",
  VITE_AZURE_REDIRECT_URI: "${VITE_AZURE_REDIRECT_URI:-}",
  VITE_AZURE_SCOPES: "${VITE_AZURE_SCOPES:-openid,profile,email}",
  VITE_AZURE_API_SCOPE: "${VITE_AZURE_API_SCOPE:-}",
};
EOF
exec nginx -g 'daemon off;'
