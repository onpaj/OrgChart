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
      throw new Error(`Failed to fetch config: ${response.status} ${response.statusText}`);
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
      throw new Error('Missing required MSAL configuration fields');
    }

    configCache = config;
    return config;
  } catch (error) {
    console.error('Error fetching frontend configuration:', error);
    throw new Error(`Failed to load application configuration: ${error instanceof Error ? error.message : 'Unknown error'}`);
  }
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