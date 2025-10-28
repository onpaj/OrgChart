import { useQuery } from '@tanstack/react-query';
import { orgChartApi } from './api';

/**
 * React Query hook for fetching organizational chart data
 */
export const useOrgChart = () => {
  return useQuery({
    queryKey: ['orgchart'],
    queryFn: () => orgChartApi.getOrganizationStructure(),
    staleTime: 1000 * 60 * 30, // Data is fresh for 30 minutes
    gcTime: 1000 * 60 * 60, // Cache data for 1 hour
    retry: 3,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
  });
};