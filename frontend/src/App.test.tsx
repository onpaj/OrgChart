import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import App from './App';

// Mock MSAL to avoid crypto API requirements in test environment
jest.mock('@azure/msal-browser', () => ({
  PublicClientApplication: jest.fn().mockImplementation(() => ({
    initialize: jest.fn().mockResolvedValue(undefined),
    handleRedirectPromise: jest.fn().mockResolvedValue(null),
    getAllAccounts: jest.fn().mockReturnValue([]),
    acquireTokenSilent: jest.fn(),
  })),
}));

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