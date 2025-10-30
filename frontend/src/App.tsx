import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MsalProvider } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';
import { msalConfig } from './auth/msalConfig';
import { AuthGuard } from './components/auth/AuthGuard';
import OrgChart from './components/OrgChart';
import './App.css';

// Create MSAL instance
const msalInstance = new PublicClientApplication(msalConfig);

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
  const [msalInitialized, setMsalInitialized] = React.useState(false);

  // Initialize MSAL and handle redirect promise
  React.useEffect(() => {
    console.log('Environment variables:', {
      clientId: process.env.REACT_APP_AZURE_CLIENT_ID,
      authority: process.env.REACT_APP_AZURE_AUTHORITY,
      backendClientId: process.env.REACT_APP_AZURE_BACKEND_CLIENT_ID
    });
    
    console.log('MSAL config:', msalConfig);
    console.log('Current URL:', window.location.href);
    
    const initializeMsal = async () => {
      try {
        await msalInstance.initialize();
        console.log('MSAL initialized successfully');
        
        // Check for error in URL
        const urlParams = new URLSearchParams(window.location.search);
        const error = urlParams.get('error');
        const errorDescription = urlParams.get('error_description');
        
        if (error) {
          console.error('Azure AD Error in URL:', {
            error,
            errorDescription,
            fullUrl: window.location.href
          });
        }
        
        const result = await msalInstance.handleRedirectPromise();
        if (result) {
          console.log('Redirect result:', result);
        } else {
          console.log('No redirect result (normal page load)');
        }
        
        setMsalInitialized(true);
      } catch (error) {
        console.error('MSAL initialization or redirect handling failed:', error);
        console.error('Error details:', {
          name: error instanceof Error ? error.name : 'Unknown',
          message: error instanceof Error ? error.message : String(error),
          stack: error instanceof Error ? error.stack : undefined
        });
        setMsalInitialized(true); // Continue even if error
      }
    };
    
    initializeMsal();
  }, []);

  if (!msalInitialized) {
    return <div>Initializing authentication...</div>;
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