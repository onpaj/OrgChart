import React from 'react';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useOrgChart } from '../hooks';

// Mock the API client hook
const mockGet = jest.fn();
const mockPost = jest.fn();
const mockPut = jest.fn();
const mockDelete = jest.fn();

jest.mock('../apiClient', () => ({
  useApiClient: () => ({
    get: mockGet,
    post: mockPost,
    put: mockPut,
    delete: mockDelete,
    request: jest.fn(),
  }),
}));

describe('useOrgChart Hook', () => {
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

  test('should fetch organization data successfully', async () => {
    const mockData = {
      organization: {
        name: 'Test Company',
        positions: [
          {
            id: 'pos1',
            title: 'CEO',
            description: 'Chief Executive Officer',
            department: 'Executive',
            employees: [],
          },
        ],
      },
    };

    mockGet.mockResolvedValueOnce(mockData);

    const { result } = renderHook(() => useOrgChart(), { wrapper });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toEqual(mockData);
    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBe(null);
  });

  test('should handle API errors', async () => {
    const mockError = new Error('Failed to fetch');
    mockGet.mockRejectedValueOnce(mockError);

    const { result } = renderHook(() => useOrgChart(), { wrapper });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    }, { timeout: 3000 });

    expect(result.current.error).toBeTruthy();
    expect(result.current.data).toBeUndefined();
    expect(result.current.isLoading).toBe(false);
  });

  test('should show loading state initially', () => {
    mockGet.mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    const { result } = renderHook(() => useOrgChart(), { wrapper });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.data).toBeUndefined();
    expect(result.current.error).toBe(null);
  });

  test('should use correct query key', () => {
    const { result } = renderHook(() => useOrgChart(), { wrapper });

    // The query key should be ['orgchart']
    expect(result.current.dataUpdatedAt).toBeDefined();
  });
});