import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_BACKEND_URL || 'http://localhost:8001';

const api = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Health check
export const checkHealth = () => api.get('/health');

// Tenants
export const getTenants = () => api.get('/tenants');
export const getTenantBySlug = (slug) => api.get(`/tenants/${slug}`);
export const createTenant = (data) => api.post('/tenants', data);

// Services
export const getTenantServices = (tenantId) => api.get(`/tenants/${tenantId}/services`);
export const createService = (tenantId, data) => api.post(`/tenants/${tenantId}/services`, data);

// Providers
export const getTenantProviders = (tenantId) => api.get(`/tenants/${tenantId}/providers`);
export const createProvider = (tenantId, data) => api.post(`/tenants/${tenantId}/providers`, data);

// Availability
export const getAvailability = (tenantId, providerId, serviceId, date, slotInterval = 15) =>
  api.get(`/availability/${tenantId}/${providerId}/${serviceId}/${date}?slotInterval=${slotInterval}`);

// Appointments
export const holdSlot = (data) => api.post('/appointments/hold', data);
export const confirmAppointment = (appointmentId, data) => 
  api.post(`/appointments/${appointmentId}/confirm`, data);
export const cancelAppointment = (appointmentId, reason) => 
  api.post(`/appointments/${appointmentId}/cancel`, { reason });

export default api;
