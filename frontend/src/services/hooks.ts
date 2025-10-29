import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orgChartApi } from './api';
import {
  CreatePositionRequest,
  UpdatePositionRequest,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
} from '../types/orgchart';

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

/**
 * React Query mutation hook for creating a position
 */
export const useCreatePosition = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreatePositionRequest) => orgChartApi.createPosition(request),
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

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdatePositionRequest }) =>
      orgChartApi.updatePosition(id, request),
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

  return useMutation({
    mutationFn: (id: string) => orgChartApi.deletePosition(id),
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

  return useMutation({
    mutationFn: (request: CreateEmployeeRequest) => orgChartApi.createEmployee(request),
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

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateEmployeeRequest }) =>
      orgChartApi.updateEmployee(id, request),
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

  return useMutation({
    mutationFn: (id: string) => orgChartApi.deleteEmployee(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orgchart'] });
    },
  });
};