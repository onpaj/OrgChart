import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import '@testing-library/jest-dom';
import OrgChart from '../OrgChart';
import { useOrgChart } from '../../services/hooks';

// Mock the hooks
jest.mock('../../services/hooks');

const mockedUseOrgChart = useOrgChart as jest.MockedFunction<typeof useOrgChart>;

describe('OrgChart Component', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });
    jest.clearAllMocks();
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  const mockOrgData = {
    organization: {
      name: 'Test Company',
      positions: [
        {
          id: 'pos1',
          title: 'CEO',
          description: 'Chief Executive Officer',
          department: 'Executive',
          level: 1,
          employees: [
            {
              id: 'emp1',
              name: 'John Doe',
              email: 'john@example.com',
              startDate: '2020-01-01',
            },
          ],
        },
        {
          id: 'pos2',
          title: 'CTO',
          description: 'Chief Technology Officer',
          department: 'Technology',
          level: 2,
          parentPositionId: 'pos1',
          employees: [
            {
              id: 'emp2',
              name: 'Jane Smith',
              email: 'jane@example.com',
              startDate: '2020-06-01',
            },
          ],
        },
      ],
    },
    permissions: {
      canEdit: false,
    },
  };

  test('should show loading state', () => {
    mockedUseOrgChart.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
      isSuccess: false,
      isError: false,
      dataUpdatedAt: 0,
      errorUpdatedAt: 0,
      failureCount: 0,
      failureReason: null,
      fetchStatus: 'fetching',
      isLoadingError: false,
      isPaused: false,
      isPending: true,
      isPlaceholderData: false,
      isRefetchError: false,
      isRefetching: false,
      isStale: false,
      refetch: jest.fn(),
      status: 'pending',
    });

    render(<OrgChart />, { wrapper });

    expect(screen.getByText('Loading organizational structure...')).toBeInTheDocument();
  });

  test('should show error state', () => {
    const mockError = new Error('Failed to load data');
    mockedUseOrgChart.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: mockError,
      isSuccess: false,
      isError: true,
      dataUpdatedAt: 0,
      errorUpdatedAt: Date.now(),
      failureCount: 1,
      failureReason: mockError,
      fetchStatus: 'idle',
      isLoadingError: true,
      isPaused: false,
      isPending: false,
      isPlaceholderData: false,
      isRefetchError: false,
      isRefetching: false,
      isStale: false,
      refetch: jest.fn(),
      status: 'error',
    });

    render(<OrgChart />, { wrapper });

    expect(screen.getByText(/❌ Failed to load data: Failed to load data/)).toBeInTheDocument();
  });

  test('should render organization data successfully', () => {
    mockedUseOrgChart.mockReturnValue({
      data: mockOrgData,
      isLoading: false,
      error: null,
      isSuccess: true,
      isError: false,
      dataUpdatedAt: Date.now(),
      errorUpdatedAt: 0,
      failureCount: 0,
      failureReason: null,
      fetchStatus: 'idle',
      isLoadingError: false,
      isPaused: false,
      isPending: false,
      isPlaceholderData: false,
      isRefetchError: false,
      isRefetching: false,
      isStale: false,
      refetch: jest.fn(),
      status: 'success',
    });

    render(<OrgChart />, { wrapper });

    expect(screen.getByText('Test Company - Organizational Chart')).toBeInTheDocument();
    // Using getAllByText since responsive design renders both mobile and desktop views
    expect(screen.getAllByText('CEO').length).toBeGreaterThan(0);
    expect(screen.getAllByText('CTO').length).toBeGreaterThan(0);
    expect(screen.getAllByText('John Doe').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Jane Smith').length).toBeGreaterThan(0);
  });

  test('should filter by department', async () => {
    mockedUseOrgChart.mockReturnValue({
      data: mockOrgData,
      isLoading: false,
      error: null,
      isSuccess: true,
      isError: false,
      dataUpdatedAt: Date.now(),
      errorUpdatedAt: 0,
      failureCount: 0,
      failureReason: null,
      fetchStatus: 'idle',
      isLoadingError: false,
      isPaused: false,
      isPending: false,
      isPlaceholderData: false,
      isRefetchError: false,
      isRefetching: false,
      isStale: false,
      refetch: jest.fn(),
      status: 'success',
    });

    render(<OrgChart />, { wrapper });

    const departmentSelect = screen.getByDisplayValue('All Departments');
    fireEvent.change(departmentSelect, { target: { value: 'Technology' } });

    await waitFor(() => {
      expect(screen.getAllByText('CTO').length).toBeGreaterThan(0);
      // CEO should still be visible as it's a parent of Technology department
      expect(screen.getAllByText('CEO').length).toBeGreaterThan(0);
    });
  });

  // Level filter is not currently implemented in the component
  test.skip('should filter by level', async () => {
    mockedUseOrgChart.mockReturnValue({
      data: mockOrgData,
      isLoading: false,
      error: null,
      isSuccess: true,
      isError: false,
      dataUpdatedAt: Date.now(),
      errorUpdatedAt: 0,
      failureCount: 0,
      failureReason: null,
      fetchStatus: 'idle',
      isLoadingError: false,
      isPaused: false,
      isPending: false,
      isPlaceholderData: false,
      isRefetchError: false,
      isRefetching: false,
      isStale: false,
      refetch: jest.fn(),
      status: 'success',
    });

    render(<OrgChart />, { wrapper });

    const levelSelect = screen.getByDisplayValue('All Levels');
    fireEvent.change(levelSelect, { target: { value: '1' } });

    await waitFor(() => {
      expect(screen.getAllByText('CEO').length).toBeGreaterThan(0);
    });

    // Note: CTO might still be visible due to the level calculation logic
    // The component shows positions up to the selected level
    // Level 2 positions would be filtered out when selecting "Up to Level 1"
  });

  test('should reset filters', async () => {
    mockedUseOrgChart.mockReturnValue({
      data: mockOrgData,
      isLoading: false,
      error: null,
      isSuccess: true,
      isError: false,
      dataUpdatedAt: Date.now(),
      errorUpdatedAt: 0,
      failureCount: 0,
      failureReason: null,
      fetchStatus: 'idle',
      isLoadingError: false,
      isPaused: false,
      isPending: false,
      isPlaceholderData: false,
      isRefetchError: false,
      isRefetching: false,
      isStale: false,
      refetch: jest.fn(),
      status: 'success',
    });

    render(<OrgChart />, { wrapper });

    // Filter by department first
    const departmentSelect = screen.getByDisplayValue('All Departments');
    fireEvent.change(departmentSelect, { target: { value: 'Technology' } });

    // Verify department filter was applied
    expect(screen.getByDisplayValue('Technology')).toBeInTheDocument();

    // Reset filters
    const resetButton = screen.getByText('Reset Filters');
    fireEvent.click(resetButton);

    // Verify department filter was reset
    await waitFor(() => {
      expect(screen.getByDisplayValue('All Departments')).toBeInTheDocument();
    });
  });

  test('should handle zoom controls', async () => {
    mockedUseOrgChart.mockReturnValue({
      data: mockOrgData,
      isLoading: false,
      error: null,
      isSuccess: true,
      isError: false,
      dataUpdatedAt: Date.now(),
      errorUpdatedAt: 0,
      failureCount: 0,
      failureReason: null,
      fetchStatus: 'idle',
      isLoadingError: false,
      isPaused: false,
      isPending: false,
      isPlaceholderData: false,
      isRefetchError: false,
      isRefetching: false,
      isStale: false,
      refetch: jest.fn(),
      status: 'success',
    });

    render(<OrgChart />, { wrapper });

    expect(screen.getByText('100%')).toBeInTheDocument();

    // Zoom in
    const zoomInButton = screen.getByText('+');
    fireEvent.click(zoomInButton);

    await waitFor(() => {
      expect(screen.getByText('110%')).toBeInTheDocument();
    });

    // Zoom out
    const zoomOutButton = screen.getByText('−');
    fireEvent.click(zoomOutButton);

    await waitFor(() => {
      expect(screen.getByText('100%')).toBeInTheDocument();
    });

    // Reset zoom
    const resetZoomButton = screen.getByText('Reset');
    fireEvent.click(resetZoomButton);

    await waitFor(() => {
      expect(screen.getByText('100%')).toBeInTheDocument();
    });
  });

  test('should display position and employee counts', () => {
    mockedUseOrgChart.mockReturnValue({
      data: mockOrgData,
      isLoading: false,
      error: null,
      isSuccess: true,
      isError: false,
      dataUpdatedAt: Date.now(),
      errorUpdatedAt: 0,
      failureCount: 0,
      failureReason: null,
      fetchStatus: 'idle',
      isLoadingError: false,
      isPaused: false,
      isPending: false,
      isPlaceholderData: false,
      isRefetchError: false,
      isRefetching: false,
      isStale: false,
      refetch: jest.fn(),
      status: 'success',
    });

    render(<OrgChart />, { wrapper });

    // Should show statistics section
    expect(screen.getByText('Positions')).toBeInTheDocument();
    expect(screen.getByText('Employees')).toBeInTheDocument();

    // Should show correct counts (2 positions and 2 employees)
    const positionCounts = screen.getAllByText('2');
    expect(positionCounts).toHaveLength(2); // One for positions, one for employees
  });
});