import { Configuration, PopupRequest } from '@azure/msal-browser';
import { FrontendConfig } from '../services/configService';

/**
 * Creates MSAL configuration from dynamic config
 */
export function createMsalConfig(config: FrontendConfig): Configuration {
  return {
    auth: {
      clientId: config.msal.clientId,
      authority: config.msal.authority,
      redirectUri: window.location.origin,
      postLogoutRedirectUri: window.location.origin,
      navigateToLoginRequestUrl: false, // Prevent navigation issues
    },
    cache: {
      cacheLocation: "localStorage", // Change to localStorage for development
      storeAuthStateInCookie: true, // Enable for better compatibility
    },
  };
}

/**
 * Creates API request configuration from dynamic config
 */
export function createApiRequest(config: FrontendConfig): PopupRequest {
  return {
    scopes: [`api://${config.msal.backendClientId}/access_as_user`],
  };
}

/**
 * Creates user request configuration
 */
export function createUserRequest(): PopupRequest {
  return {
    scopes: ["openid", "profile"],
  };
}

// Legacy exports for backwards compatibility (will be removed once App.tsx is updated)
export const msalConfig: Configuration = {
  auth: {
    clientId: process.env.REACT_APP_AZURE_CLIENT_ID || '',
    authority: process.env.REACT_APP_AZURE_AUTHORITY || '',
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    navigateToLoginRequestUrl: false,
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: true,
  },
};

export const apiRequest: PopupRequest = {
  scopes: [`api://${process.env.REACT_APP_AZURE_BACKEND_CLIENT_ID || ''}/access_as_user`],
};

export const userRequest: PopupRequest = {
  scopes: ["openid", "profile"],
};