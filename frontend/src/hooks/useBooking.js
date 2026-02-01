import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as api from '../lib/api';

// Health check hook
export function useHealth() {
  return useQuery({
    queryKey: ['health'],
    queryFn: () => api.checkHealth().then(res => res.data),
  });
}

// Tenants hooks
export function useTenants() {
  return useQuery({
    queryKey: ['tenants'],
    queryFn: () => api.getTenants().then(res => res.data),
  });
}

export function useTenant(slug) {
  return useQuery({
    queryKey: ['tenant', slug],
    queryFn: () => api.getTenantBySlug(slug).then(res => res.data),
    enabled: !!slug,
  });
}

export function useCreateTenant() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: api.createTenant,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tenants'] }),
  });
}

// Services hooks
export function useTenantServices(tenantId) {
  return useQuery({
    queryKey: ['services', tenantId],
    queryFn: () => api.getTenantServices(tenantId).then(res => res.data),
    enabled: !!tenantId,
  });
}

export function useCreateService(tenantId) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data) => api.createService(tenantId, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['services', tenantId] }),
  });
}

// Providers hooks
export function useTenantProviders(tenantId) {
  return useQuery({
    queryKey: ['providers', tenantId],
    queryFn: () => api.getTenantProviders(tenantId).then(res => res.data),
    enabled: !!tenantId,
  });
}

export function useCreateProvider(tenantId) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data) => api.createProvider(tenantId, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['providers', tenantId] }),
  });
}

// Availability hooks
export function useAvailability(tenantId, providerId, serviceId, date) {
  return useQuery({
    queryKey: ['availability', tenantId, providerId, serviceId, date],
    queryFn: () => api.getAvailability(tenantId, providerId, serviceId, date).then(res => res.data),
    enabled: !!(tenantId && providerId && serviceId && date),
    staleTime: 30000, // 30 seconds
  });
}

// Appointment hooks
export function useHoldSlot() {
  return useMutation({
    mutationFn: api.holdSlot,
  });
}

export function useConfirmAppointment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ appointmentId, ...data }) => api.confirmAppointment(appointmentId, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['availability'] }),
  });
}

export function useCancelAppointment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ appointmentId, reason }) => api.cancelAppointment(appointmentId, reason),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['availability'] }),
  });
}
