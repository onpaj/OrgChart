/**
 * Authentication service for MS Entra ID integration
 * Handles token retrieval and management for API calls
 */

interface AuthInfo {
  access_token?: string;
  user_claims?: Array<{ typ: string; val: string }>;
  user_id?: string;
}

interface AuthResponse {
  clientPrincipal?: {
    identityProvider: string;
    userId: string;
    userDetails: string;
    userRoles: string[];
    claims: Array<{ typ: string; val: string }>;
  };
}

/**
 * Gets authentication information from Azure App Service Easy Auth
 */
export async function getAuthInfo(): Promise<AuthInfo | null> {
  try {
    // Try to get auth info from Azure App Service Easy Auth endpoint
    const response = await fetch('/.auth/me');
    if (!response.ok) {
      console.warn('Unable to fetch auth info from /.auth/me:', response.status);
      return null;
    }

    const authData: AuthResponse[] = await response.json();
    if (!authData || authData.length === 0 || !authData[0].clientPrincipal) {
      console.warn('No client principal found in auth response');
      return null;
    }

    const principal = authData[0].clientPrincipal;
    
    return {
      user_claims: principal.claims,
      user_id: principal.userId,
    };
  } catch (error) {
    console.warn('Error fetching auth info:', error);
    return null;
  }
}

/**
 * Gets the access token for API calls
 * For Azure App Service Easy Auth, we may not need a traditional bearer token
 * since the authentication is handled at the App Service level
 */
export async function getAccessToken(): Promise<string | null> {
  try {
    // For Azure App Service Easy Auth, we might not need to send a Bearer token
    // The authentication headers are automatically added by Azure App Service
    
    // Method 1: Try to get token from /.auth/refresh endpoint
    try {
      const refreshResponse = await fetch('/.auth/refresh', {
        method: 'POST',
        credentials: 'include'
      });
      
      if (refreshResponse.ok) {
        // Check if we can get a token from the response
        const refreshData = await refreshResponse.json().catch(() => null);
        if (refreshData?.access_token) {
          return refreshData.access_token;
        }
      }
    } catch (error) {
      console.log('Refresh token method failed:', error);
    }
    
    // Method 2: Check if we're in development and return null to rely on Easy Auth headers
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
      console.log('Development environment detected, skipping token retrieval');
      return null;
    }
    
    // Method 3: For Azure App Service, we may not need a token at all
    // Easy Auth handles authentication at the service level
    console.log('Azure App Service Easy Auth detected, relying on service-level authentication');
    return null;
    
  } catch (error) {
    console.warn('Error getting access token:', error);
    return null;
  }
}


/**
 * Checks if user has a specific claim
 */
export async function hasClaimValue(claimType: string, claimValue?: string): Promise<boolean> {
  try {
    const authInfo = await getAuthInfo();
    if (!authInfo?.user_claims) {
      return false;
    }

    const claim = authInfo.user_claims.find(c => c.typ === claimType);
    if (!claim) {
      return false;
    }

    // If no specific value is required, just check if claim exists
    if (claimValue === undefined) {
      return true;
    }

    return claim.val === claimValue;
  } catch (error) {
    console.warn('Error checking claim:', error);
    return false;
  }
}

/**
 * Checks if user has the OrgChart_Write claim
 */
export async function canEditOrgChart(): Promise<boolean> {
  return hasClaimValue('OrgChart_Write');
}