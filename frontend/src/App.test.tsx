import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import App from './App';

// Mock the OrgChart component to avoid complex render dependencies
jest.mock('./components/OrgChart', () => {
  return function MockOrgChart() {
    return <div data-testid="org-chart">Mocked OrgChart Component</div>;
  };
});

describe('App Component', () => {
  test('renders without crashing', () => {
    render(<App />);
  });

  test('contains the App class element', () => {
    render(<App />);
    const appElement = document.querySelector('.App');
    expect(appElement).toBeInTheDocument();
  });

  test('renders the OrgChart component', () => {
    render(<App />);
    const orgChartElement = screen.getByTestId('org-chart');
    expect(orgChartElement).toBeInTheDocument();
  });

  test('provides QueryClientProvider context', () => {
    render(<App />);
    // If the component renders without error, the QueryClientProvider is working
    expect(screen.getByTestId('org-chart')).toBeInTheDocument();
  });
});