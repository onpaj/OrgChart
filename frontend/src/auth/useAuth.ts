import { useCallback, useState } from 'react';
import { useMsal } from '@azure/msal-react';
import { AccountInfo, InteractionRequiredAuthError } from '@azure/msal-browser';
import { createApiRequest, createUserRequest } from './msalConfig';
import { getConfig } from '../services/configService';

interface AuthContextType {
  user: AccountInfo | null;
  isAuthenticated: boolean;
  login: () => Promise<void>;
  logout: () => Promise<void>;
  getAccessToken: () => Promise<string | null>;
  isLoading: boolean;
}

// Token cache with expiration
interface TokenCache {
  token: string;
  expiresAt: number;
}

let tokenCache: TokenCache | null = null;

export const useAuth = (): AuthContextType => {
  const { instance, accounts, inProgress } = useMsal();
  const [isLoading, setIsLoading] = useState(false);
  
  const account = accounts[0] || null;
  const isAuthenticated = !!account;

  // Login function with redirect flow - use basic scopes first
  const login = useCallback(async () => {
    setIsLoading(true);
    try {
      const userRequest = createUserRequest();
      console.log('Starting login with request:', userRequest);
      await instance.loginRedirect(userRequest);
    } catch (error) {
      console.error('Login failed:', error);
      console.error('Error details:', {
        name: error instanceof Error ? error.name : 'Unknown',
        message: error instanceof Error ? error.message : String(error),
        stack: error instanceof Error ? error.stack : undefined
      });
      setIsLoading(false);
      throw error;
    }
    // Note: redirect doesn't return here, so we don't set loading to false
  }, [instance]);

  // Logout function
  const logout = useCallback(async () => {
    setIsLoading(true);
    try {
      tokenCache = null; // Clear token cache
      await instance.logoutRedirect();
    } catch (error) {
      console.error('Logout failed:', error);
      setIsLoading(false);
      throw error;
    }
    // Note: redirect doesn't return here, so we don't set loading to false
  }, [instance]);

  // Token acquisition with caching
  const getAccessToken = useCallback(async (): Promise<string | null> => {
    console.log('getAccessToken called, account:', account);
    if (!account) {
      console.log('No account available for token acquisition');
      return null;
    }

    // Check cached token
    if (tokenCache && Date.now() < tokenCache.expiresAt) {
      console.log('Using cached token');
      return tokenCache.token;
    }

    try {
      const config = getConfig();
      const apiRequest = createApiRequest(config);
      console.log('Acquiring new token with scopes:', apiRequest.scopes);
      
      // Try silent token acquisition
      const response = await instance.acquireTokenSilent({
        ...apiRequest,
        account,
      });
      
      console.log('Token acquired successfully, length:', response.accessToken.length);
      
      // Cache token for 55 minutes (5-minute buffer before expiration)
      tokenCache = {
        token: response.accessToken,
        expiresAt: Date.now() + 55 * 60 * 1000
      };
      
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        // Fallback to redirect for interactive token acquisition
        try {
          const config = getConfig();
          const apiRequest = createApiRequest(config);
          await instance.acquireTokenRedirect({
            ...apiRequest,
            account,
          });
          // This will redirect, so we won't get here
          return null;
        } catch (redirectError) {
          console.error('Token acquisition failed:', redirectError);
          return null;
        }
      }
      
      console.error('Token acquisition failed:', error);
      console.error('Error details:', {
        name: error instanceof Error ? error.name : 'Unknown',
        message: error instanceof Error ? error.message : String(error),
        stack: error instanceof Error ? error.stack : undefined
      });
      return null;
    }
  }, [instance, account]);

  return {
    user: account,
    isAuthenticated,
    login,
    logout,
    getAccessToken,
    isLoading: isLoading || inProgress !== 'none',
  };
};