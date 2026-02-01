import React, { useState } from 'react';
import { Plus, Building, Briefcase, Users, Settings, ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import { motion } from 'framer-motion';
import { Button } from '../components/ui/button';
import { Card } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '../components/ui/dialog';
import { useLanguage, LanguageToggle } from '../lib/i18n';
import { 
  useTenants, 
  useCreateTenant,
  useTenantServices,
  useCreateService,
  useTenantProviders,
  useCreateProvider
} from '../hooks/useBooking';

function AdminPage() {
  const { t, language } = useLanguage();
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
  const [tenantForm, setTenantForm] = useState({ name: '', slug: '', timeZoneId: 'America/Sao_Paulo', bufferMinutes: 15, language: 'pt' });
  const [serviceForm, setServiceForm] = useState({ name: '', durationMinutes: 60, price: 50, description: '' });
  const [providerForm, setProviderForm] = useState({ name: '', email: '', serviceIds: [] });
  
  const [openDialog, setOpenDialog] = useState(null);

  const handleCreateTenant = async () => {
    try {
      await createTenantMutation.mutateAsync(tenantForm);
      toast.success(language === 'pt' ? 'Inquilino criado!' : 'Tenant created!');
      setTenantForm({ name: '', slug: '', timeZoneId: 'America/Sao_Paulo', bufferMinutes: 15, language: 'pt' });
      setOpenDialog(null);
    } catch (error) {
      toast.error(error.response?.data || (language === 'pt' ? 'Falha ao criar inquilino' : 'Failed to create tenant'));
    }
  };

  const handleCreateService = async () => {
    try {
      await createServiceMutation.mutateAsync(serviceForm);
      toast.success(language === 'pt' ? 'Serviço criado!' : 'Service created!');
      setServiceForm({ name: '', durationMinutes: 60, price: 50, description: '' });
      setOpenDialog(null);
    } catch (error) {
      toast.error(error.response?.data || (language === 'pt' ? 'Falha ao criar serviço' : 'Failed to create service'));
    }
  };

  const handleCreateProvider = async () => {
    try {
      await createProviderMutation.mutateAsync(providerForm);
      toast.success(language === 'pt' ? 'Profissional criado!' : 'Provider created!');
      setProviderForm({ name: '', email: '', serviceIds: [] });
      setOpenDialog(null);
    } catch (error) {
      toast.error(error.response?.data || (language === 'pt' ? 'Falha ao criar profissional' : 'Failed to create provider'));
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-900 to-slate-950">
      {/* Header */}
      <header className="glass sticky top-0 z-50 border-b border-slate-800/50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 flex justify-between items-center">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-violet-600 to-violet-700 flex items-center justify-center">
              <Settings className="w-5 h-5 text-white" />
            </div>
            <div>
              <h1 className="font-bold text-white font-[Manrope]">{t('adminDashboard')}</h1>
              <p className="text-xs text-slate-500">{t('manageTenants')}</p>
            </div>
          </div>
          <div className="flex items-center gap-4">
            <LanguageToggle />
            <a href="/" className="flex items-center gap-2 text-sm text-slate-400 hover:text-violet-400 transition-colors">
              <ArrowLeft className="w-4 h-4" />
              {t('backToBooking')}
            </a>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid md:grid-cols-3 gap-6">
          {/* Tenants Column */}
          <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0 }}>
            <Card className="bg-slate-900 border-slate-800 rounded-2xl overflow-hidden">
              <div className="p-5 border-b border-slate-800 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-violet-600/10 flex items-center justify-center">
                    <Building className="w-5 h-5 text-violet-400" />
                  </div>
                  <div>
                    <h2 className="font-semibold text-white font-[Manrope]">{t('tenants')}</h2>
                    <p className="text-xs text-slate-500">{tenants?.length || 0} {language === 'pt' ? 'registrados' : 'registered'}</p>
                  </div>
                </div>
                <Dialog open={openDialog === 'tenant'} onOpenChange={(open) => setOpenDialog(open ? 'tenant' : null)}>
                  <DialogTrigger asChild>
                    <Button size="icon" variant="ghost" className="rounded-xl hover:bg-violet-600/10">
                      <Plus className="w-4 h-4 text-violet-400" />
                    </Button>
                  </DialogTrigger>
                  <DialogContent className="bg-slate-900 border-slate-800 rounded-2xl">
                    <DialogHeader>
                      <DialogTitle className="text-white font-[Manrope]">{t('createTenant')}</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4 pt-4">
                      <div className="space-y-2">
                        <Label className="text-slate-300">{t('businessName')}</Label>
                        <Input
                          value={tenantForm.name}
                          onChange={(e) => setTenantForm({ ...tenantForm, name: e.target.value })}
                          placeholder="Salão Beleza"
                          className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label className="text-slate-300">{t('slug')}</Label>
                        <Input
                          value={tenantForm.slug}
                          onChange={(e) => setTenantForm({ ...tenantForm, slug: e.target.value.toLowerCase().replace(/\s+/g, '-') })}
                          placeholder="salao-beleza"
                          className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label className="text-slate-300">{t('language')}</Label>
                        <select
                          value={tenantForm.language}
                          onChange={(e) => setTenantForm({ ...tenantForm, language: e.target.value })}
                          className="w-full bg-slate-950 border border-slate-800 h-12 rounded-lg px-3 text-white"
                        >
                          <option value="pt">Português (BR)</option>
                          <option value="en">English</option>
                        </select>
                      </div>
                      <div className="space-y-2">
                        <Label className="text-slate-300">{t('timezone')}</Label>
                        <Input
                          value={tenantForm.timeZoneId}
                          onChange={(e) => setTenantForm({ ...tenantForm, timeZoneId: e.target.value })}
                          placeholder="America/Sao_Paulo"
                          className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label className="text-slate-300">{t('bufferMinutes')}</Label>
                        <Input
                          type="number"
                          value={tenantForm.bufferMinutes}
                          onChange={(e) => setTenantForm({ ...tenantForm, bufferMinutes: parseInt(e.target.value) || 0 })}
                          className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                        />
                      </div>
                      <Button 
                        onClick={handleCreateTenant} 
                        disabled={createTenantMutation.isPending}
                        className="w-full bg-violet-600 hover:bg-violet-500 rounded-full h-12 font-semibold"
                      >
                        {createTenantMutation.isPending ? (
                          <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                        ) : null}
                        {t('createTenant')}
                      </Button>
                    </div>
                  </DialogContent>
                </Dialog>
              </div>
              <div className="p-4 max-h-96 overflow-y-auto">
                {tenantsLoading ? (
                  <div className="flex justify-center py-8">
                    <div className="w-6 h-6 border-2 border-violet-600 border-t-transparent rounded-full animate-spin" />
                  </div>
                ) : tenants?.length === 0 ? (
                  <p className="text-slate-500 text-center py-8 text-sm">{language === 'pt' ? 'Nenhum inquilino' : 'No tenants'}</p>
                ) : (
                  <div className="space-y-2">
                    {tenants?.map(tenant => (
                      <button
                        key={tenant.id}
                        onClick={() => setSelectedTenant(tenant)}
                        className={`w-full p-4 rounded-xl text-left transition-all duration-300
                          ${selectedTenant?.id === tenant.id 
                            ? 'bg-violet-600 text-white glow-violet-sm' 
                            : 'bg-slate-800/50 hover:bg-slate-800 text-slate-300'}`}
                        data-testid={`tenant-${tenant.id}`}
                      >
                        <div className="font-medium">{tenant.name}</div>
                        <div className="text-xs opacity-70 font-mono">/{tenant.slug}</div>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            </Card>
          </motion.div>

          {/* Services Column */}
          <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }}>
            <Card className="bg-slate-900 border-slate-800 rounded-2xl overflow-hidden">
              <div className="p-5 border-b border-slate-800 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-emerald-600/10 flex items-center justify-center">
                    <Briefcase className="w-5 h-5 text-emerald-400" />
                  </div>
                  <div>
                    <h2 className="font-semibold text-white font-[Manrope]">{t('services')}</h2>
                    <p className="text-xs text-slate-500">{selectedTenant ? `${services?.length || 0} ${language === 'pt' ? 'serviços' : 'services'}` : t('selectTenantFirst')}</p>
                  </div>
                </div>
                {selectedTenant && (
                  <Dialog open={openDialog === 'service'} onOpenChange={(open) => setOpenDialog(open ? 'service' : null)}>
                    <DialogTrigger asChild>
                      <Button size="icon" variant="ghost" className="rounded-xl hover:bg-emerald-600/10">
                        <Plus className="w-4 h-4 text-emerald-400" />
                      </Button>
                    </DialogTrigger>
                    <DialogContent className="bg-slate-900 border-slate-800 rounded-2xl">
                      <DialogHeader>
                        <DialogTitle className="text-white font-[Manrope]">{t('createService')}</DialogTitle>
                      </DialogHeader>
                      <div className="space-y-4 pt-4">
                        <div className="space-y-2">
                          <Label className="text-slate-300">{t('serviceName')}</Label>
                          <Input
                            value={serviceForm.name}
                            onChange={(e) => setServiceForm({ ...serviceForm, name: e.target.value })}
                            placeholder={language === 'pt' ? 'Corte de Cabelo' : 'Haircut'}
                            className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                          />
                        </div>
                        <div className="space-y-2">
                          <Label className="text-slate-300">{t('durationMinutes')}</Label>
                          <Input
                            type="number"
                            value={serviceForm.durationMinutes}
                            onChange={(e) => setServiceForm({ ...serviceForm, durationMinutes: parseInt(e.target.value) || 30 })}
                            className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                          />
                        </div>
                        <div className="space-y-2">
                          <Label className="text-slate-300">{t('priceAmount')} ($)</Label>
                          <Input
                            type="number"
                            value={serviceForm.price}
                            onChange={(e) => setServiceForm({ ...serviceForm, price: parseFloat(e.target.value) || 0 })}
                            className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                          />
                        </div>
                        <Button 
                          onClick={handleCreateService} 
                          disabled={createServiceMutation.isPending}
                          className="w-full bg-emerald-600 hover:bg-emerald-500 rounded-full h-12 font-semibold"
                        >
                          {createServiceMutation.isPending ? (
                            <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                          ) : null}
                          {t('createService')}
                        </Button>
                      </div>
                    </DialogContent>
                  </Dialog>
                )}
              </div>
              <div className="p-4 max-h-96 overflow-y-auto">
                {!selectedTenant ? (
                  <p className="text-slate-500 text-center py-8 text-sm">{t('selectTenantFirst')}</p>
                ) : services?.length === 0 ? (
                  <p className="text-slate-500 text-center py-8 text-sm">{language === 'pt' ? 'Nenhum serviço' : 'No services'}</p>
                ) : (
                  <div className="space-y-2">
                    {services?.map(service => (
                      <div key={service.id} className="p-4 rounded-xl bg-slate-800/50" data-testid={`service-item-${service.id}`}>
                        <div className="font-medium text-white">{service.name}</div>
                        <div className="text-xs text-slate-400 mt-1">
                          {service.durationMinutes} min • ${service.price?.amount || 0}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </Card>
          </motion.div>

          {/* Providers Column */}
          <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
            <Card className="bg-slate-900 border-slate-800 rounded-2xl overflow-hidden">
              <div className="p-5 border-b border-slate-800 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-amber-600/10 flex items-center justify-center">
                    <Users className="w-5 h-5 text-amber-400" />
                  </div>
                  <div>
                    <h2 className="font-semibold text-white font-[Manrope]">{t('providers')}</h2>
                    <p className="text-xs text-slate-500">{selectedTenant ? `${providers?.length || 0} ${language === 'pt' ? 'profissionais' : 'providers'}` : t('selectTenantFirst')}</p>
                  </div>
                </div>
                {selectedTenant && services?.length > 0 && (
                  <Dialog open={openDialog === 'provider'} onOpenChange={(open) => setOpenDialog(open ? 'provider' : null)}>
                    <DialogTrigger asChild>
                      <Button size="icon" variant="ghost" className="rounded-xl hover:bg-amber-600/10">
                        <Plus className="w-4 h-4 text-amber-400" />
                      </Button>
                    </DialogTrigger>
                    <DialogContent className="bg-slate-900 border-slate-800 rounded-2xl">
                      <DialogHeader>
                        <DialogTitle className="text-white font-[Manrope]">{t('createProvider')}</DialogTitle>
                      </DialogHeader>
                      <div className="space-y-4 pt-4">
                        <div className="space-y-2">
                          <Label className="text-slate-300">{t('providerName')}</Label>
                          <Input
                            value={providerForm.name}
                            onChange={(e) => setProviderForm({ ...providerForm, name: e.target.value })}
                            placeholder="João Silva"
                            className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                          />
                        </div>
                        <div className="space-y-2">
                          <Label className="text-slate-300">{t('email')}</Label>
                          <Input
                            type="email"
                            value={providerForm.email}
                            onChange={(e) => setProviderForm({ ...providerForm, email: e.target.value })}
                            placeholder="joao@email.com"
                            className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                          />
                        </div>
                        <div className="space-y-2">
                          <Label className="text-slate-300">{t('services')}</Label>
                          <div className="space-y-2 max-h-40 overflow-y-auto p-2 bg-slate-950 rounded-lg">
                            {services?.map(service => (
                              <label key={service.id} className="flex items-center gap-3 text-slate-300 cursor-pointer hover:text-white transition-colors">
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
                                  className="rounded border-slate-700 bg-slate-800 text-violet-600 focus:ring-violet-500"
                                />
                                {service.name}
                              </label>
                            ))}
                          </div>
                        </div>
                        <Button 
                          onClick={handleCreateProvider} 
                          disabled={createProviderMutation.isPending}
                          className="w-full bg-amber-600 hover:bg-amber-500 rounded-full h-12 font-semibold"
                        >
                          {createProviderMutation.isPending ? (
                            <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                          ) : null}
                          {t('createProvider')}
                        </Button>
                      </div>
                    </DialogContent>
                  </Dialog>
                )}
              </div>
              <div className="p-4 max-h-96 overflow-y-auto">
                {!selectedTenant ? (
                  <p className="text-slate-500 text-center py-8 text-sm">{t('selectTenantFirst')}</p>
                ) : services?.length === 0 ? (
                  <p className="text-slate-500 text-center py-8 text-sm">{t('createServiceFirst')}</p>
                ) : providers?.length === 0 ? (
                  <p className="text-slate-500 text-center py-8 text-sm">{language === 'pt' ? 'Nenhum profissional' : 'No providers'}</p>
                ) : (
                  <div className="space-y-2">
                    {providers?.map(provider => (
                      <div key={provider.id} className="p-4 rounded-xl bg-slate-800/50" data-testid={`provider-item-${provider.id}`}>
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center text-white font-bold">
                            {provider.name.charAt(0).toUpperCase()}
                          </div>
                          <div>
                            <div className="font-medium text-white">{provider.name}</div>
                            {provider.email && (
                              <div className="text-xs text-slate-400">{provider.email}</div>
                            )}
                            <div className="text-xs text-slate-500 mt-1">
                              {provider.serviceIds?.length || 0} {t('servicesOffered')}
                            </div>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </Card>
          </motion.div>
        </div>
      </main>
    </div>
  );
}

export default AdminPage;
