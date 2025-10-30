import { Configuration, PopupRequest } from '@azure/msal-browser';

export const msalConfig: Configuration = {
  auth: {
    clientId: process.env.REACT_APP_AZURE_CLIENT_ID!,
    authority: process.env.REACT_APP_AZURE_AUTHORITY!,
    redirectUri: "http://localhost:3001",
    postLogoutRedirectUri: "http://localhost:3001",
    navigateToLoginRequestUrl: false, // Prevent navigation issues
  },
  cache: {
    cacheLocation: "localStorage", // Change to localStorage for development
    storeAuthStateInCookie: true, // Enable for better compatibility
  },
};

// Request configuration for API access - using backend API scopes
export const apiRequest: PopupRequest = {
  scopes: [`api://${process.env.REACT_APP_AZURE_BACKEND_CLIENT_ID}/access_as_user`],
};

// Request configuration for user info
export const userRequest: PopupRequest = {
  scopes: ["openid", "profile"],
};