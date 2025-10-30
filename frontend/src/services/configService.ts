/**
 * Service for fetching frontend configuration from the backend
 */

export interface MsalConfig {
  clientId: string;
  tenantId: string;
  authority: string;
  backendClientId: string;
}

export interface ApiConfig {
  baseUrl: string;
}

export interface FeatureConfig {
  authenticationEnabled: boolean;
}

export interface FrontendConfig {
  msal: MsalConfig;
  api: ApiConfig;
  features: FeatureConfig;
}

let configCache: FrontendConfig | null = null;

/**
 * Fetches the frontend configuration from the backend
 * Results are cached for the duration of the session
 * Falls back to environment variables if backend config fails
 */
export async function fetchConfig(): Promise<FrontendConfig> {
  if (configCache) {
    return configCache;
  }

  try {
    console.log('Fetching frontend configuration from backend...');
    
    // Use appropriate URL based on environment
    const configUrl = process.env.NODE_ENV === 'development' 
      ? 'http://localhost:5001/api/config' 
      : '/api/config';
    
    console.log('Config URL:', configUrl);
    
    const response = await fetch(configUrl, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      console.warn(`Config endpoint failed: ${response.status} ${response.statusText}, falling back to environment variables`);
      return createFallbackConfig();
    }

    const config: FrontendConfig = await response.json();
    
    console.log('Frontend configuration fetched successfully:', {
      hasClientId: !!config.msal.clientId,
      hasTenantId: !!config.msal.tenantId,
      hasBackendClientId: !!config.msal.backendClientId,
      authEnabled: config.features.authenticationEnabled
    });

    // Validate required fields
    if (!config.msal.clientId || !config.msal.tenantId || !config.msal.backendClientId) {
      console.warn('Backend config missing required fields, falling back to environment variables');
      return createFallbackConfig();
    }

    configCache = config;
    return config;
  } catch (error) {
    console.error('Error fetching frontend configuration:', error);
    console.log('Falling back to environment variables...');
    return createFallbackConfig();
  }
}

/**
 * Creates configuration from environment variables as fallback
 * Only works in development - production relies on backend config endpoint
 */
function createFallbackConfig(): FrontendConfig {
  // In development, try environment variables
  if (process.env.NODE_ENV === 'development') {
    const clientId = process.env.REACT_APP_AZURE_CLIENT_ID || '';
    const tenantId = process.env.REACT_APP_AZURE_TENANT_ID || '';
    const backendClientId = process.env.REACT_APP_AZURE_BACKEND_CLIENT_ID || '';
    const authority = process.env.REACT_APP_AZURE_AUTHORITY || `https://login.microsoftonline.com/${tenantId}`;

    if (clientId && tenantId && backendClientId) {
      const config: FrontendConfig = {
        msal: {
          clientId,
          tenantId,
          authority,
          backendClientId,
        },
        api: {
          baseUrl: process.env.REACT_APP_API_URL || '/api',
        },
        features: {
          authenticationEnabled: true,
        },
      };

      console.log('Using fallback configuration from environment variables:', {
        hasClientId: !!config.msal.clientId,
        hasTenantId: !!config.msal.tenantId,
        hasBackendClientId: !!config.msal.backendClientId,
        authEnabled: config.features.authenticationEnabled
      });

      configCache = config;
      return config;
    }
  }

  // In production or if dev env vars missing, config endpoint is required
  throw new Error(
    'Configuration unavailable. ' +
    (process.env.NODE_ENV === 'development' 
      ? 'Please configure REACT_APP_AZURE_* environment variables or ensure backend is running with config endpoint.' 
      : 'Please configure Frontend__ClientId and AzureAd settings in Azure Web App Application Settings.')
  );
}

/**
 * Gets the cached configuration or throws if not loaded
 */
export function getConfig(): FrontendConfig {
  if (!configCache) {
    throw new Error('Configuration not loaded. Call fetchConfig() first.');
  }
  return configCache;
}

/**
 * Clears the configuration cache
 */
export function clearConfigCache(): void {
  configCache = null;
}