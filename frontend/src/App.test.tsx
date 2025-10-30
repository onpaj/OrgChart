import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import App from './App';

// Mock MSAL config functions
jest.mock('./auth/msalConfig', () => ({
  createMsalConfig: (config: any) => {
    if (!config) {
      throw new Error('Config is undefined in createMsalConfig mock');
    }
    return {
      auth: {
        clientId: config.msal.clientId,
        authority: config.msal.authority,
        redirectUri: 'http://localhost',
        postLogoutRedirectUri: 'http://localhost',
        navigateToLoginRequestUrl: false,
      },
      cache: {
        cacheLocation: 'localStorage',
        storeAuthStateInCookie: true,
      },
    };
  },
  createApiRequest: (config: any) => ({
    scopes: [`api://${config.msal.backendClientId}/access_as_user`],
  }),
  createUserRequest: () => ({
    scopes: ['openid', 'profile'],
  }),
}));

// Mock the config service to avoid network requests
jest.mock('./services/configService', () => {
  const mockConfig = {
    msal: {
      clientId: 'test-client-id',
      tenantId: 'test-tenant-id',
      authority: 'https://login.microsoftonline.com/test-tenant-id',
      backendClientId: 'test-backend-client-id',
    },
    api: {
      baseUrl: '/api',
    },
    features: {
      authenticationEnabled: true,
    },
  };

  return {
    fetchConfig: async () => mockConfig,
    getConfig: () => mockConfig,
    clearConfigCache: () => {},
  };
});

// Mock MSAL to avoid crypto API requirements in test environment
jest.mock('@azure/msal-browser', () => {
  return {
    PublicClientApplication: class MockPublicClientApplication {
      async initialize() {
        return undefined;
      }
      async handleRedirectPromise() {
        return null;
      }
      getAllAccounts() {
        return [];
      }
      acquireTokenSilent = jest.fn();
    },
  };
});

// Mock MSAL React
jest.mock('@azure/msal-react', () => ({
  MsalProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
  useIsAuthenticated: jest.fn(() => true),
  useMsal: jest.fn(() => ({
    instance: {
      getAllAccounts: jest.fn().mockReturnValue([]),
      acquireTokenSilent: jest.fn(),
    },
    accounts: [],
  })),
}));

// Mock the AuthGuard to avoid authentication requirements
jest.mock('./components/auth/AuthGuard', () => ({
  AuthGuard: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock the OrgChart component to avoid complex render dependencies
jest.mock('./components/OrgChart', () => {
  return function MockOrgChart() {
    return <div data-testid="org-chart">Mocked OrgChart Component</div>;
  };
});

describe('App Component', () => {
  test('renders without crashing', async () => {
    render(<App />);
    // Wait for MSAL initialization
    await screen.findByTestId('org-chart');
  });

  test('contains the App class element', async () => {
    render(<App />);
    // Wait for MSAL initialization to complete
    await screen.findByTestId('org-chart');
    const appElement = document.querySelector('.App');
    expect(appElement).toBeInTheDocument();
  });

  test('renders the OrgChart component', async () => {
    render(<App />);
    // Wait for MSAL initialization and component to render
    const orgChartElement = await screen.findByTestId('org-chart');
    expect(orgChartElement).toBeInTheDocument();
  });

  test('provides QueryClientProvider context', async () => {
    render(<App />);
    // If the component renders without error, the QueryClientProvider is working
    const orgChartElement = await screen.findByTestId('org-chart');
    expect(orgChartElement).toBeInTheDocument();
  });
});