/**
 * Authentication service for MSAL integration
 * Handles token retrieval and management for API calls
 */

// Global reference to the auth hook - will be set by useApiClient hook
let globalGetAccessToken: (() => Promise<string | null>) | null = null;

/**
 * Sets the global token getter function (used by API client)
 */
export function setTokenGetter(getter: () => Promise<string | null>) {
  globalGetAccessToken = getter;
}

/**
 * Gets the access token for API calls using MSAL
 */
export async function getAccessToken(): Promise<string | null> {
  if (!globalGetAccessToken) {
    console.warn('Token getter not set - make sure to use API client within MSAL context');
    return null;
  }
  
  try {
    return await globalGetAccessToken();
  } catch (error) {
    console.warn('Error getting access token:', error);
    return null;
  }
}

/**
 * For backward compatibility - now uses MSAL token claims
 */
export async function canEditOrgChart(): Promise<boolean> {
  // With MSAL, we check if user is authenticated and has proper scopes
  // The backend will validate the actual permissions
  const token = await getAccessToken();
  return !!token;
}