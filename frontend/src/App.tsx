import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MsalProvider } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';
import { createMsalConfig } from './auth/msalConfig';
import { fetchConfig, FrontendConfig } from './services/configService';
import { AuthGuard } from './components/auth/AuthGuard';
import OrgChart from './components/OrgChart';
import './App.css';

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 3,
      refetchOnWindowFocus: false,
      staleTime: 1000 * 60 * 5, // 5 minutes
    },
  },
});

function App() {
  const [appState, setAppState] = React.useState<'loading' | 'configuring' | 'ready' | 'error'>('loading');
  const [msalInstance, setMsalInstance] = React.useState<PublicClientApplication | null>(null);
  const [config, setConfig] = React.useState<FrontendConfig | null>(null);
  const [error, setError] = React.useState<string | null>(null);

  // Initialize configuration and MSAL
  React.useEffect(() => {
    const initializeApp = async () => {
      try {
        setAppState('loading');
        console.log('Fetching application configuration...');
        
        // Fetch configuration from backend
        const appConfig = await fetchConfig();
        setConfig(appConfig);
        console.log('Configuration loaded successfully');
        
        setAppState('configuring');
        console.log('Initializing MSAL with dynamic configuration...');
        
        // Create MSAL instance with dynamic config
        const msalConfig = createMsalConfig(appConfig);
        console.log('MSAL config created:', {
          clientId: msalConfig.auth.clientId.substring(0, 8) + '...',
          authority: msalConfig.auth.authority,
          redirectUri: msalConfig.auth.redirectUri
        });
        
        const msalInstanceNew = new PublicClientApplication(msalConfig);
        await msalInstanceNew.initialize();
        console.log('MSAL initialized successfully');
        
        // Check for error in URL
        const urlParams = new URLSearchParams(window.location.search);
        const authError = urlParams.get('error');
        const errorDescription = urlParams.get('error_description');
        
        if (authError) {
          console.error('Azure AD Error in URL:', {
            error: authError,
            errorDescription,
            fullUrl: window.location.href
          });
        }
        
        // Handle redirect promise
        const result = await msalInstanceNew.handleRedirectPromise();
        if (result) {
          console.log('Redirect result:', result);
        } else {
          console.log('No redirect result (normal page load)');
        }
        
        setMsalInstance(msalInstanceNew);
        setAppState('ready');
        
      } catch (error) {
        console.error('App initialization failed:', error);
        setError(error instanceof Error ? error.message : 'Unknown error occurred');
        setAppState('error');
      }
    };
    
    initializeApp();
  }, []);

  if (appState === 'loading') {
    return <div>Loading application configuration...</div>;
  }

  if (appState === 'configuring') {
    return <div>Initializing authentication...</div>;
  }

  if (appState === 'error') {
    return (
      <div>
        <h2>Application Error</h2>
        <p>Failed to initialize the application: {error}</p>
        <button onClick={() => window.location.reload()}>Retry</button>
      </div>
    );
  }

  if (!msalInstance || !config) {
    return <div>Initializing...</div>;
  }

  return (
    <MsalProvider instance={msalInstance}>
      <QueryClientProvider client={queryClient}>
        <AuthGuard>
          <div className="App">
            <OrgChart />
          </div>
        </AuthGuard>
      </QueryClientProvider>
    </MsalProvider>
  );
}

export default App;