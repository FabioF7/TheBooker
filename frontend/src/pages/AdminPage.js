import React, { useState } from 'react';
import { Plus, Loader2, Building, Briefcase, Users } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '../components/ui/dialog';
import { 
  useTenants, 
  useCreateTenant,
  useTenantServices,
  useCreateService,
  useTenantProviders,
  useCreateProvider
} from '../hooks/useBooking';

function AdminPage() {
  const [selectedTenant, setSelectedTenant] = useState(null);
  
  // Queries
  const { data: tenants, isLoading: tenantsLoading } = useTenants();
  const { data: services } = useTenantServices(selectedTenant?.id);
  const { data: providers } = useTenantProviders(selectedTenant?.id);
  
  // Mutations
  const createTenantMutation = useCreateTenant();
  const createServiceMutation = useCreateService(selectedTenant?.id);
  const createProviderMutation = useCreateProvider(selectedTenant?.id);

  // Form state
  const [tenantForm, setTenantForm] = useState({ name: '', slug: '', timeZoneId: 'America/New_York', bufferMinutes: 15 });
  const [serviceForm, setServiceForm] = useState({ name: '', durationMinutes: 60, price: 50, description: '' });
  const [providerForm, setProviderForm] = useState({ name: '', email: '', serviceIds: [] });
  
  const [openDialog, setOpenDialog] = useState(null);

  const handleCreateTenant = async () => {
    try {
      const response = await createTenantMutation.mutateAsync(tenantForm);
      toast.success('Tenant created!');
      setTenantForm({ name: '', slug: '', timeZoneId: 'America/New_York', bufferMinutes: 15 });
      setOpenDialog(null);
    } catch (error) {
      toast.error(error.response?.data || 'Failed to create tenant');
    }
  };

  const handleCreateService = async () => {
    try {
      await createServiceMutation.mutateAsync(serviceForm);
      toast.success('Service created!');
      setServiceForm({ name: '', durationMinutes: 60, price: 50, description: '' });
      setOpenDialog(null);
    } catch (error) {
      toast.error(error.response?.data || 'Failed to create service');
    }
  };

  const handleCreateProvider = async () => {
    try {
      await createProviderMutation.mutateAsync(providerForm);
      toast.success('Provider created!');
      setProviderForm({ name: '', email: '', serviceIds: [] });
      setOpenDialog(null);
    } catch (error) {
      toast.error(error.response?.data || 'Failed to create provider');
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 py-8 px-4">
      <div className="max-w-6xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-white">Admin Dashboard</h1>
            <p className="text-slate-400">Manage tenants, services, and providers</p>
          </div>
          <a href="/" className="text-blue-400 hover:text-blue-300">← Back to Booking</a>
        </div>

        <div className="grid md:grid-cols-3 gap-6">
          {/* Tenants */}
          <Card className="bg-slate-900 border-slate-800">
            <CardHeader className="flex flex-row items-center justify-between">
              <div>
                <CardTitle className="text-white flex items-center gap-2">
                  <Building className="w-5 h-5" />
                  Tenants
                </CardTitle>
                <CardDescription className="text-slate-400">
                  {tenants?.length || 0} tenants
                </CardDescription>
              </div>
              <Dialog open={openDialog === 'tenant'} onOpenChange={(open) => setOpenDialog(open ? 'tenant' : null)}>
                <DialogTrigger asChild>
                  <Button size="icon" variant="ghost">
                    <Plus className="w-4 h-4" />
                  </Button>
                </DialogTrigger>
                <DialogContent className="bg-slate-900 border-slate-800">
                  <DialogHeader>
                    <DialogTitle className="text-white">Create Tenant</DialogTitle>
                  </DialogHeader>
                  <div className="space-y-4 pt-4">
                    <div className="space-y-2">
                      <Label className="text-slate-300">Business Name</Label>
                      <Input
                        value={tenantForm.name}
                        onChange={(e) => setTenantForm({ ...tenantForm, name: e.target.value })}
                        placeholder="Acme Salon"
                        className="bg-slate-800 border-slate-700"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-slate-300">Slug (URL)</Label>
                      <Input
                        value={tenantForm.slug}
                        onChange={(e) => setTenantForm({ ...tenantForm, slug: e.target.value.toLowerCase().replace(/\s+/g, '-') })}
                        placeholder="acme-salon"
                        className="bg-slate-800 border-slate-700"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-slate-300">Timezone</Label>
                      <Input
                        value={tenantForm.timeZoneId}
                        onChange={(e) => setTenantForm({ ...tenantForm, timeZoneId: e.target.value })}
                        placeholder="America/New_York"
                        className="bg-slate-800 border-slate-700"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-slate-300">Buffer Minutes</Label>
                      <Input
                        type="number"
                        value={tenantForm.bufferMinutes}
                        onChange={(e) => setTenantForm({ ...tenantForm, bufferMinutes: parseInt(e.target.value) || 0 })}
                        className="bg-slate-800 border-slate-700"
                      />
                    </div>
                    <Button 
                      onClick={handleCreateTenant} 
                      disabled={createTenantMutation.isPending}
                      className="w-full"
                    >
                      {createTenantMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                      Create Tenant
                    </Button>
                  </div>
                </DialogContent>
              </Dialog>
            </CardHeader>
            <CardContent>
              {tenantsLoading ? (
                <div className="flex justify-center py-4">
                  <Loader2 className="w-5 h-5 animate-spin text-blue-500" />
                </div>
              ) : (
                <div className="space-y-2">
                  {tenants?.map(tenant => (
                    <button
                      key={tenant.id}
                      onClick={() => setSelectedTenant(tenant)}
                      className={`w-full p-3 rounded-lg text-left transition-colors
                        ${selectedTenant?.id === tenant.id 
                          ? 'bg-blue-600 text-white' 
                          : 'bg-slate-800/50 hover:bg-slate-800 text-slate-300'}`}
                    >
                      <div className="font-medium">{tenant.name}</div>
                      <div className="text-xs opacity-70">/{tenant.slug}</div>
                    </button>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Services */}
          <Card className="bg-slate-900 border-slate-800">
            <CardHeader className="flex flex-row items-center justify-between">
              <div>
                <CardTitle className="text-white flex items-center gap-2">
                  <Briefcase className="w-5 h-5" />
                  Services
                </CardTitle>
                <CardDescription className="text-slate-400">
                  {selectedTenant ? `${services?.length || 0} services` : 'Select a tenant'}
                </CardDescription>
              </div>
              {selectedTenant && (
                <Dialog open={openDialog === 'service'} onOpenChange={(open) => setOpenDialog(open ? 'service' : null)}>
                  <DialogTrigger asChild>
                    <Button size="icon" variant="ghost">
                      <Plus className="w-4 h-4" />
                    </Button>
                  </DialogTrigger>
                  <DialogContent className="bg-slate-900 border-slate-800">
                    <DialogHeader>
                      <DialogTitle className="text-white">Create Service</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4 pt-4">
                      <div className="space-y-2">
                        <Label className="text-slate-300">Service Name</Label>
                        <Input
                          value={serviceForm.name}
                          onChange={(e) => setServiceForm({ ...serviceForm, name: e.target.value })}
                          placeholder="Haircut"
                          className="bg-slate-800 border-slate-700"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label className="text-slate-300">Duration (minutes)</Label>
                        <Input
                          type="number"
                          value={serviceForm.durationMinutes}
                          onChange={(e) => setServiceForm({ ...serviceForm, durationMinutes: parseInt(e.target.value) || 30 })}
                          className="bg-slate-800 border-slate-700"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label className="text-slate-300">Price ($)</Label>
                        <Input
                          type="number"
                          value={serviceForm.price}
                          onChange={(e) => setServiceForm({ ...serviceForm, price: parseFloat(e.target.value) || 0 })}
                          className="bg-slate-800 border-slate-700"
                        />
                      </div>
                      <Button 
                        onClick={handleCreateService} 
                        disabled={createServiceMutation.isPending}
                        className="w-full"
                      >
                        {createServiceMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                        Create Service
                      </Button>
                    </div>
                  </DialogContent>
                </Dialog>
              )}
            </CardHeader>
            <CardContent>
              {!selectedTenant ? (
                <p className="text-slate-500 text-center py-4">Select a tenant first</p>
              ) : (
                <div className="space-y-2">
                  {services?.map(service => (
                    <div key={service.id} className="p-3 rounded-lg bg-slate-800/50">
                      <div className="font-medium text-white">{service.name}</div>
                      <div className="text-xs text-slate-400">
                        {service.durationMinutes} min • ${service.price?.amount || 0}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Providers */}
          <Card className="bg-slate-900 border-slate-800">
            <CardHeader className="flex flex-row items-center justify-between">
              <div>
                <CardTitle className="text-white flex items-center gap-2">
                  <Users className="w-5 h-5" />
                  Providers
                </CardTitle>
                <CardDescription className="text-slate-400">
                  {selectedTenant ? `${providers?.length || 0} providers` : 'Select a tenant'}
                </CardDescription>
              </div>
              {selectedTenant && services?.length > 0 && (
                <Dialog open={openDialog === 'provider'} onOpenChange={(open) => setOpenDialog(open ? 'provider' : null)}>
                  <DialogTrigger asChild>
                    <Button size="icon" variant="ghost">
                      <Plus className="w-4 h-4" />
                    </Button>
                  </DialogTrigger>
                  <DialogContent className="bg-slate-900 border-slate-800">
                    <DialogHeader>
                      <DialogTitle className="text-white">Create Provider</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4 pt-4">
                      <div className="space-y-2">
                        <Label className="text-slate-300">Name</Label>
                        <Input
                          value={providerForm.name}
                          onChange={(e) => setProviderForm({ ...providerForm, name: e.target.value })}
                          placeholder="John Smith"
                          className="bg-slate-800 border-slate-700"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label className="text-slate-300">Email</Label>
                        <Input
                          type="email"
                          value={providerForm.email}
                          onChange={(e) => setProviderForm({ ...providerForm, email: e.target.value })}
                          placeholder="john@example.com"
                          className="bg-slate-800 border-slate-700"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label className="text-slate-300">Services</Label>
                        <div className="space-y-2">
                          {services?.map(service => (
                            <label key={service.id} className="flex items-center gap-2 text-slate-300">
                              <input
                                type="checkbox"
                                checked={providerForm.serviceIds.includes(service.id)}
                                onChange={(e) => {
                                  if (e.target.checked) {
                                    setProviderForm({ ...providerForm, serviceIds: [...providerForm.serviceIds, service.id] });
                                  } else {
                                    setProviderForm({ ...providerForm, serviceIds: providerForm.serviceIds.filter(id => id !== service.id) });
                                  }
                                }}
                                className="rounded"
                              />
                              {service.name}
                            </label>
                          ))}
                        </div>
                      </div>
                      <Button 
                        onClick={handleCreateProvider} 
                        disabled={createProviderMutation.isPending}
                        className="w-full"
                      >
                        {createProviderMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                        Create Provider
                      </Button>
                    </div>
                  </DialogContent>
                </Dialog>
              )}
            </CardHeader>
            <CardContent>
              {!selectedTenant ? (
                <p className="text-slate-500 text-center py-4">Select a tenant first</p>
              ) : (
                <div className="space-y-2">
                  {providers?.map(provider => (
                    <div key={provider.id} className="p-3 rounded-lg bg-slate-800/50">
                      <div className="font-medium text-white">{provider.name}</div>
                      {provider.email && (
                        <div className="text-xs text-slate-400">{provider.email}</div>
                      )}
                      <div className="text-xs text-slate-500 mt-1">
                        {provider.serviceIds?.length || 0} services assigned
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

export default AdminPage;
