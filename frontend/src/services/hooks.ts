import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useApiClient } from './apiClient';
import {
  CreatePositionRequest,
  UpdatePositionRequest,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
  OrgChartResponse,
  Position,
  Employee,
  GraphUserInfo,
  UserPhotoResponse,
} from '../types/orgchart';

/**
 * React Query hook for fetching organizational chart data
 */
export const useOrgChart = () => {
  const apiClient = useApiClient();
  
  return useQuery({
    queryKey: ['orgchart'],
    queryFn: (): Promise<OrgChartResponse> => apiClient.get('/orgchart'),
    staleTime: 1000 * 60 * 30, // Data is fresh for 30 minutes
    gcTime: 1000 * 60 * 60, // Cache data for 1 hour
    retry: 3,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
  });
};

/**
 * React Query mutation hook for creating a position
 */
export const useCreatePosition = () => {
  const queryClient = useQueryClient();
  const apiClient = useApiClient();

  return useMutation({
    mutationFn: (request: CreatePositionRequest): Promise<Position> => 
      apiClient.post('/orgchart/positions', request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orgchart'] });
    },
  });
};

/**
 * React Query mutation hook for updating a position
 */
export const useUpdatePosition = () => {
  const queryClient = useQueryClient();
  const apiClient = useApiClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdatePositionRequest }): Promise<Position> =>
      apiClient.put(`/orgchart/positions/${id}`, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orgchart'] });
    },
  });
};

/**
 * React Query mutation hook for deleting a position
 */
export const useDeletePosition = () => {
  const queryClient = useQueryClient();
  const apiClient = useApiClient();

  return useMutation({
    mutationFn: (id: string): Promise<void> => 
      apiClient.delete(`/orgchart/positions/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orgchart'] });
    },
  });
};

/**
 * React Query mutation hook for creating an employee
 */
export const useCreateEmployee = () => {
  const queryClient = useQueryClient();
  const apiClient = useApiClient();

  return useMutation({
    mutationFn: (request: CreateEmployeeRequest): Promise<Employee> => 
      apiClient.post('/orgchart/employees', request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orgchart'] });
    },
  });
};

/**
 * React Query mutation hook for updating an employee
 */
export const useUpdateEmployee = () => {
  const queryClient = useQueryClient();
  const apiClient = useApiClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateEmployeeRequest }): Promise<Employee> =>
      apiClient.put(`/orgchart/employees/${id}`, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orgchart'] });
    },
  });
};

/**
 * React Query mutation hook for deleting an employee
 */
export const useDeleteEmployee = () => {
  const queryClient = useQueryClient();
  const apiClient = useApiClient();

  return useMutation({
    mutationFn: (id: string): Promise<void> => 
      apiClient.delete(`/orgchart/employees/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orgchart'] });
    },
  });
};

/**
 * React Query hook for fetching user profile from Microsoft Graph
 */
export const useUserProfile = (email: string, enabled: boolean = true) => {
  const apiClient = useApiClient();
  
  return useQuery({
    queryKey: ['user-profile', email],
    queryFn: (): Promise<GraphUserInfo> => apiClient.get(`/user/profile?email=${encodeURIComponent(email)}`),
    enabled: enabled && !!email,
    staleTime: 1000 * 60 * 30, // Data is fresh for 30 minutes
    gcTime: 1000 * 60 * 60, // Cache data for 1 hour
    retry: (failureCount, error: any) => {
      // Don't retry on 404 (user not found)
      if (error?.message?.includes('404')) {
        return false;
      }
      return failureCount < 2;
    },
  });
};

/**
 * React Query hook for fetching user photo from Microsoft Graph
 */
export const useUserPhoto = (email: string, enabled: boolean = true) => {
  const apiClient = useApiClient();
  
  return useQuery({
    queryKey: ['user-photo', email],
    queryFn: (): Promise<UserPhotoResponse> => apiClient.get(`/user/photo?email=${encodeURIComponent(email)}`),
    enabled: enabled && !!email,
    staleTime: 1000 * 60 * 60, // Photos are fresh for 1 hour
    gcTime: 1000 * 60 * 60 * 24, // Cache photos for 24 hours
    retry: (failureCount, error: any) => {
      // Don't retry on 404 (photo not found)
      if (error?.message?.includes('404')) {
        return false;
      }
      return failureCount < 2;
    },
  });
};

/**
 * React Query hook for fetching multiple user profiles in batch
 */
export const useUserProfilesBatch = (emails: string[], enabled: boolean = true) => {
  const apiClient = useApiClient();
  
  return useQuery({
    queryKey: ['user-profiles-batch', emails.sort()], // Sort for consistent caching
    queryFn: (): Promise<Record<string, GraphUserInfo | null>> => 
      apiClient.post('/user/profiles/batch', emails),
    enabled: enabled && emails.length > 0,
    staleTime: 1000 * 60 * 30, // Data is fresh for 30 minutes
    gcTime: 1000 * 60 * 60, // Cache data for 1 hour
    retry: 2,
  });
};