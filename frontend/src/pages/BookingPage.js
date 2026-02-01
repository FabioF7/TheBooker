import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Calendar, Clock, User, Check, Sparkles, ArrowRight, ChevronLeft } from 'lucide-react';
import { format, startOfToday, addDays } from 'date-fns';
import { ptBR, enUS } from 'date-fns/locale';
import { toast } from 'sonner';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '../components/ui/button';
import { Card } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Calendar as CalendarComponent } from '../components/ui/calendar';
import { useLanguage, LanguageToggle } from '../lib/i18n';
import { 
  useTenants, 
  useTenantServices, 
  useTenantProviders, 
  useAvailability,
  useHoldSlot,
  useConfirmAppointment 
} from '../hooks/useBooking';

const STEPS = ['service', 'provider', 'datetime', 'confirm'];

const stepVariants = {
  initial: { opacity: 0, x: 20 },
  animate: { opacity: 1, x: 0 },
  exit: { opacity: 0, x: -20 }
};

function BookingPage() {
  const { tenantSlug } = useParams();
  const { t, language } = useLanguage();
  const dateLocale = language === 'pt' ? ptBR : enUS;
  
  const [currentStep, setCurrentStep] = useState(0);
  const [sessionId] = useState(() => `session-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`);
  
  // Selection state
  const [selectedTenant, setSelectedTenant] = useState(null);
  const [selectedService, setSelectedService] = useState(null);
  const [selectedProvider, setSelectedProvider] = useState(null);
  const [selectedDate, setSelectedDate] = useState(null);
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [heldAppointment, setHeldAppointment] = useState(null);
  const [expiresAt, setExpiresAt] = useState(null);
  
  // Customer form
  const [customerName, setCustomerName] = useState('');
  const [customerEmail, setCustomerEmail] = useState('');
  const [customerPhone, setCustomerPhone] = useState('');
  const [notes, setNotes] = useState('');
  
  // Booking complete state
  const [bookingComplete, setBookingComplete] = useState(false);
  const [confirmedBooking, setConfirmedBooking] = useState(null);

  // Queries
  const { data: tenants } = useTenants();
  const { data: services, isLoading: servicesLoading } = useTenantServices(selectedTenant?.id);
  const { data: providers, isLoading: providersLoading } = useTenantProviders(selectedTenant?.id);
  const { data: availability, isLoading: availabilityLoading, refetch: refetchAvailability } = useAvailability(
    selectedTenant?.id,
    selectedProvider?.id,
    selectedService?.id,
    selectedDate ? format(selectedDate, 'yyyy-MM-dd') : null
  );

  // Mutations
  const holdSlotMutation = useHoldSlot();
  const confirmMutation = useConfirmAppointment();

  // Auto-select tenant
  useEffect(() => {
    if (tenants && tenantSlug) {
      const tenant = tenants.find(t => t.slug === tenantSlug);
      if (tenant) setSelectedTenant(tenant);
    } else if (tenants && tenants.length === 1) {
      setSelectedTenant(tenants[0]);
    } else if (tenants && tenants.length > 0) {
      setSelectedTenant(tenants[0]);
    }
  }, [tenants, tenantSlug]);

  // Filter providers that offer selected service
  const availableProviders = providers?.filter(p => 
    selectedService && p.serviceIds?.includes(selectedService.id)
  ) || [];

  // Handle slot selection and hold
  const handleSlotSelect = async (slot) => {
    if (!slot.isAvailable) return;
    
    setSelectedSlot(slot);
    
    try {
      const response = await holdSlotMutation.mutateAsync({
        tenantId: selectedTenant.id,
        serviceId: selectedService.id,
        providerId: selectedProvider.id,
        date: format(selectedDate, 'yyyy-MM-dd'),
        startTime: slot.startTime,
        sessionId
      });
      
      setHeldAppointment(response.data);
      setExpiresAt(new Date(response.data.expiresAt));
      setCurrentStep(3);
      toast.success(language === 'pt' ? 'Horário reservado por 10 minutos!' : 'Slot held for 10 minutes!');
    } catch (error) {
      toast.error(error.response?.data?.detail || t('slotNotAvailable'));
      setSelectedSlot(null);
      refetchAvailability();
    }
  };

  // Handle booking confirmation
  const handleConfirm = async () => {
    if (!heldAppointment) return;
    
    try {
      const response = await confirmMutation.mutateAsync({
        appointmentId: heldAppointment.appointmentId,
        sessionId,
        customerName,
        customerEmail,
        customerPhone: customerPhone || null,
        notes: notes || null
      });
      
      setConfirmedBooking(response.data);
      setBookingComplete(true);
      toast.success(t('bookingConfirmed'));
    } catch (error) {
      toast.error(error.response?.data?.detail || t('bookingFailed'));
    }
  };

  // Countdown timer
  const [timeRemaining, setTimeRemaining] = useState(null);
  
  useEffect(() => {
    if (!expiresAt) return;
    
    const interval = setInterval(() => {
      const remaining = Math.max(0, Math.floor((expiresAt - new Date()) / 1000));
      setTimeRemaining(remaining);
      
      if (remaining === 0) {
        toast.error(t('holdExpired'));
        setHeldAppointment(null);
        setExpiresAt(null);
        setSelectedSlot(null);
        setCurrentStep(2);
        refetchAvailability();
      }
    }, 1000);
    
    return () => clearInterval(interval);
  }, [expiresAt, refetchAvailability, t]);

  // Success Screen
  if (bookingComplete && confirmedBooking) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4 bg-gradient-to-b from-slate-900 to-slate-950">
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          className="w-full max-w-md"
        >
          <Card className="bg-slate-900/80 backdrop-blur-xl border-slate-800 p-8 rounded-2xl" data-testid="booking-confirmed-card">
            <div className="text-center">
              <motion.div 
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                transition={{ delay: 0.2, type: 'spring' }}
                className="mx-auto w-20 h-20 rounded-full bg-gradient-to-br from-emerald-500 to-emerald-600 flex items-center justify-center mb-6 shadow-lg shadow-emerald-500/30"
              >
                <Check className="w-10 h-10 text-white" />
              </motion.div>
              <h1 className="text-2xl font-bold text-white mb-2 font-[Manrope]">{t('bookingConfirmed')}</h1>
              <p className="text-slate-400 mb-8">{t('bookingConfirmedDesc')}</p>
            </div>
            
            <div className="bg-slate-800/50 rounded-xl p-5 space-y-3 mb-6">
              <div className="flex justify-between">
                <span className="text-slate-400">{t('service')}</span>
                <span className="text-white font-medium">{selectedService?.name}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">{t('professional')}</span>
                <span className="text-white font-medium">{selectedProvider?.name}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">{t('date')}</span>
                <span className="text-white font-medium">{format(selectedDate, 'PPP', { locale: dateLocale })}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">{t('time')}</span>
                <span className="text-white font-medium font-mono">{selectedSlot?.startTime?.slice(0,5)} - {selectedSlot?.endTime?.slice(0,5)}</span>
              </div>
            </div>
            
            <p className="text-sm text-slate-500 text-center mb-6">
              {t('confirmationSent')} <span className="text-violet-400">{customerEmail}</span>
            </p>
            
            <Button 
              className="w-full bg-violet-600 hover:bg-violet-500 rounded-full h-12 font-semibold" 
              onClick={() => window.location.reload()}
              data-testid="book-another-btn"
            >
              {t('bookAnother')}
            </Button>
          </Card>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-900 to-slate-950">
      {/* Header */}
      <header className="glass sticky top-0 z-50 border-b border-slate-800/50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 flex justify-between items-center">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-violet-600 to-violet-700 flex items-center justify-center">
              <Sparkles className="w-5 h-5 text-white" />
            </div>
            <div>
              <h1 className="font-bold text-white font-[Manrope]">{selectedTenant?.name || 'The Booker'}</h1>
              <p className="text-xs text-slate-500">{t('bookingSubtitle')}</p>
            </div>
          </div>
          <div className="flex items-center gap-4">
            <LanguageToggle />
            <a href="/admin" className="text-sm text-slate-400 hover:text-violet-400 transition-colors">
              Admin
            </a>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Progress Steps */}
        <div className="flex justify-center mb-12">
          <div className="flex items-center gap-3">
            {STEPS.map((step, index) => (
              <React.Fragment key={step}>
                <button
                  onClick={() => index < currentStep && setCurrentStep(index)}
                  disabled={index > currentStep}
                  className={`relative flex items-center justify-center w-10 h-10 rounded-full font-semibold text-sm transition-all duration-300
                    ${index === currentStep 
                      ? 'bg-violet-600 text-white glow-violet-sm scale-110' 
                      : index < currentStep 
                        ? 'bg-violet-600/20 text-violet-400 hover:bg-violet-600/30 cursor-pointer'
                        : 'bg-slate-800 text-slate-500'}`}
                  data-testid={`step-${step}`}
                >
                  {index < currentStep ? <Check className="w-4 h-4" /> : index + 1}
                </button>
                {index < STEPS.length - 1 && (
                  <div className={`w-12 h-0.5 transition-colors duration-300 ${index < currentStep ? 'bg-violet-600' : 'bg-slate-800'}`} />
                )}
              </React.Fragment>
            ))}
          </div>
        </div>

        {/* Step Content */}
        <AnimatePresence mode="wait">
          {/* Step 1: Services */}
          {currentStep === 0 && (
            <motion.div
              key="step-service"
              variants={stepVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={{ duration: 0.3 }}
              data-testid="service-selection"
            >
              <div className="text-center mb-8">
                <h2 className="text-3xl font-bold text-white mb-2 font-[Manrope]">{t('selectService')}</h2>
                <p className="text-slate-400">{t('selectServiceDesc')}</p>
              </div>

              {servicesLoading ? (
                <div className="flex justify-center py-16">
                  <div className="w-8 h-8 border-2 border-violet-600 border-t-transparent rounded-full animate-spin" />
                </div>
              ) : services?.length === 0 ? (
                <p className="text-slate-500 text-center py-16">{t('noServices')}</p>
              ) : (
                <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
                  {services?.map(service => (
                    <Card
                      key={service.id}
                      onClick={() => {
                        setSelectedService(service);
                        setCurrentStep(1);
                      }}
                      className="bg-slate-900 border-slate-800 hover:border-violet-500/50 p-6 rounded-2xl cursor-pointer card-hover group"
                      data-testid={`service-${service.id}`}
                    >
                      <div className="flex items-start justify-between mb-4">
                        <div className="w-12 h-12 rounded-xl bg-violet-600/10 flex items-center justify-center group-hover:bg-violet-600/20 transition-colors">
                          <Calendar className="w-6 h-6 text-violet-400" />
                        </div>
                        <ArrowRight className="w-5 h-5 text-slate-600 group-hover:text-violet-400 transition-colors" />
                      </div>
                      <h3 className="text-lg font-semibold text-white mb-2 font-[Manrope]">{service.name}</h3>
                      <p className="text-sm text-slate-400 mb-4">{service.description || ''}</p>
                      <div className="flex items-center justify-between pt-4 border-t border-slate-800">
                        <span className="text-sm text-slate-500 font-mono">{service.durationMinutes} {t('minutes')}</span>
                        <span className="text-lg font-bold text-violet-400">${service.price?.amount || 0}</span>
                      </div>
                    </Card>
                  ))}
                </div>
              )}
            </motion.div>
          )}

          {/* Step 2: Providers */}
          {currentStep === 1 && (
            <motion.div
              key="step-provider"
              variants={stepVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={{ duration: 0.3 }}
              data-testid="provider-selection"
            >
              <div className="text-center mb-8">
                <h2 className="text-3xl font-bold text-white mb-2 font-[Manrope]">{t('selectProvider')}</h2>
                <p className="text-slate-400">{t('selectProviderDesc')}</p>
              </div>

              {providersLoading ? (
                <div className="flex justify-center py-16">
                  <div className="w-8 h-8 border-2 border-violet-600 border-t-transparent rounded-full animate-spin" />
                </div>
              ) : availableProviders.length === 0 ? (
                <p className="text-slate-500 text-center py-16">{t('noProviders')}</p>
              ) : (
                <div className="max-w-2xl mx-auto space-y-4">
                  {availableProviders.map(provider => (
                    <Card
                      key={provider.id}
                      onClick={() => {
                        setSelectedProvider(provider);
                        setSelectedDate(addDays(startOfToday(), 1));
                        setCurrentStep(2);
                      }}
                      className="bg-slate-900 border-slate-800 hover:border-violet-500/50 p-5 rounded-2xl cursor-pointer card-hover group"
                      data-testid={`provider-${provider.id}`}
                    >
                      <div className="flex items-center gap-4">
                        <div className="w-16 h-16 rounded-2xl bg-gradient-to-br from-violet-600 to-violet-700 flex items-center justify-center text-2xl font-bold text-white shadow-lg shadow-violet-900/30">
                          {provider.name.charAt(0).toUpperCase()}
                        </div>
                        <div className="flex-1">
                          <h3 className="text-lg font-semibold text-white font-[Manrope]">{provider.name}</h3>
                          {provider.email && (
                            <p className="text-sm text-slate-400">{provider.email}</p>
                          )}
                          <p className="text-xs text-slate-500 mt-1">{provider.serviceIds?.length || 0} {t('servicesOffered')}</p>
                        </div>
                        <ArrowRight className="w-5 h-5 text-slate-600 group-hover:text-violet-400 transition-colors" />
                      </div>
                    </Card>
                  ))}
                </div>
              )}

              <div className="flex justify-center mt-8">
                <Button variant="ghost" onClick={() => setCurrentStep(0)} className="text-slate-400">
                  <ChevronLeft className="w-4 h-4 mr-2" />
                  {t('back')}
                </Button>
              </div>
            </motion.div>
          )}

          {/* Step 3: Date & Time */}
          {currentStep === 2 && (
            <motion.div
              key="step-datetime"
              variants={stepVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={{ duration: 0.3 }}
            >
              <div className="text-center mb-8">
                <h2 className="text-3xl font-bold text-white mb-2 font-[Manrope]">{t('selectDateTime')}</h2>
                <p className="text-slate-400">{t('selectDateTimeDesc')}</p>
              </div>

              <div className="grid lg:grid-cols-2 gap-8 max-w-5xl mx-auto">
                {/* Calendar */}
                <Card className="bg-slate-900 border-slate-800 p-6 rounded-2xl" data-testid="date-selection">
                  <h3 className="text-lg font-semibold text-white mb-4 font-[Manrope]">{t('selectDate')}</h3>
                  <CalendarComponent
                    mode="single"
                    selected={selectedDate}
                    onSelect={setSelectedDate}
                    disabled={(date) => date < startOfToday()}
                    locale={dateLocale}
                    className="rounded-xl border-0"
                  />
                </Card>

                {/* Time Slots */}
                <Card className="bg-slate-900 border-slate-800 p-6 rounded-2xl" data-testid="time-selection">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-white font-[Manrope]">{t('availableTimes')}</h3>
                    {selectedDate && (
                      <span className="text-sm text-slate-400">{format(selectedDate, 'EEEE, d MMM', { locale: dateLocale })}</span>
                    )}
                  </div>

                  {availabilityLoading ? (
                    <div className="flex justify-center py-12">
                      <div className="w-8 h-8 border-2 border-violet-600 border-t-transparent rounded-full animate-spin" />
                    </div>
                  ) : !availability?.isOpen ? (
                    <div className="text-center py-12">
                      <Clock className="w-12 h-12 text-slate-700 mx-auto mb-3" />
                      <p className="text-slate-500">{availability?.closedReason || t('closedDay')}</p>
                    </div>
                  ) : availability?.slots?.length === 0 ? (
                    <p className="text-slate-500 text-center py-12">{t('noSlots')}</p>
                  ) : (
                    <div className="grid grid-cols-4 gap-2 max-h-80 overflow-y-auto pr-2">
                      {availability?.slots?.map((slot, index) => (
                        <button
                          key={index}
                          onClick={() => handleSlotSelect(slot)}
                          disabled={!slot.isAvailable || holdSlotMutation.isPending}
                          className={`time-slot px-3 py-2.5 text-sm font-mono rounded-lg transition-all
                            ${slot.isAvailable 
                              ? 'bg-slate-800 hover:bg-violet-600 text-white hover:scale-105' 
                              : 'bg-slate-800/30 text-slate-600 cursor-not-allowed line-through'}`}
                          data-testid={`slot-${slot.startTime}`}
                        >
                          {slot.startTime?.slice(0, 5)}
                        </button>
                      ))}
                    </div>
                  )}
                </Card>
              </div>

              <div className="flex justify-center mt-8">
                <Button variant="ghost" onClick={() => setCurrentStep(1)} className="text-slate-400">
                  <ChevronLeft className="w-4 h-4 mr-2" />
                  {t('back')}
                </Button>
              </div>
            </motion.div>
          )}

          {/* Step 4: Confirm */}
          {currentStep === 3 && heldAppointment && (
            <motion.div
              key="step-confirm"
              variants={stepVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={{ duration: 0.3 }}
              className="max-w-4xl mx-auto"
              data-testid="confirm-booking"
            >
              <div className="text-center mb-8">
                <h2 className="text-3xl font-bold text-white mb-2 font-[Manrope]">{t('yourDetails')}</h2>
                <p className="text-slate-400">{t('yourDetailsDesc')}</p>
                {timeRemaining !== null && (
                  <div className={`inline-flex items-center gap-2 mt-4 px-4 py-2 rounded-full ${timeRemaining < 60 ? 'bg-rose-500/10 text-rose-400' : 'bg-violet-500/10 text-violet-400'}`}>
                    <Clock className="w-4 h-4" />
                    <span className="font-mono">{t('slotHeld')} {Math.floor(timeRemaining / 60)}:{String(timeRemaining % 60).padStart(2, '0')}</span>
                  </div>
                )}
              </div>

              <div className="grid lg:grid-cols-5 gap-8">
                {/* Form */}
                <Card className="lg:col-span-3 bg-slate-900 border-slate-800 p-6 rounded-2xl">
                  <div className="space-y-5">
                    <div className="space-y-2">
                      <Label className="text-slate-300">{t('fullName')} *</Label>
                      <Input
                        value={customerName}
                        onChange={(e) => setCustomerName(e.target.value)}
                        placeholder="Maria Silva"
                        className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                        data-testid="customer-name-input"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-slate-300">{t('email')} *</Label>
                      <Input
                        type="email"
                        value={customerEmail}
                        onChange={(e) => setCustomerEmail(e.target.value)}
                        placeholder="maria@email.com"
                        className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                        data-testid="customer-email-input"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-slate-300">{t('phone')} ({t('optional')})</Label>
                      <Input
                        type="tel"
                        value={customerPhone}
                        onChange={(e) => setCustomerPhone(e.target.value)}
                        placeholder="+55 11 99999-9999"
                        className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                        data-testid="customer-phone-input"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-slate-300">{t('notes')} ({t('optional')})</Label>
                      <Input
                        value={notes}
                        onChange={(e) => setNotes(e.target.value)}
                        placeholder={t('notesPlaceholder')}
                        className="bg-slate-950 border-slate-800 h-12 rounded-lg"
                        data-testid="customer-notes-input"
                      />
                    </div>
                  </div>
                </Card>

                {/* Summary */}
                <Card className="lg:col-span-2 bg-slate-900 border-slate-800 p-6 rounded-2xl h-fit">
                  <h3 className="text-lg font-semibold text-white mb-4 font-[Manrope]">{t('bookingSummary')}</h3>
                  <div className="space-y-3 mb-6">
                    <div className="flex justify-between">
                      <span className="text-slate-400">{t('service')}</span>
                      <span className="text-white font-medium">{selectedService?.name}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-slate-400">{t('professional')}</span>
                      <span className="text-white font-medium">{selectedProvider?.name}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-slate-400">{t('date')}</span>
                      <span className="text-white font-medium">{selectedDate && format(selectedDate, 'dd/MM/yyyy')}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-slate-400">{t('time')}</span>
                      <span className="text-white font-medium font-mono">{selectedSlot?.startTime?.slice(0,5)} - {selectedSlot?.endTime?.slice(0,5)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-slate-400">{t('duration')}</span>
                      <span className="text-white font-medium">{selectedService?.durationMinutes} {t('minutes')}</span>
                    </div>
                    <div className="flex justify-between pt-3 border-t border-slate-800">
                      <span className="text-slate-400">{t('total')}</span>
                      <span className="text-xl font-bold text-violet-400">${selectedService?.price?.amount || 0}</span>
                    </div>
                  </div>

                  <div className="space-y-3">
                    <Button 
                      onClick={handleConfirm}
                      disabled={!customerName || !customerEmail || confirmMutation.isPending}
                      className="w-full bg-violet-600 hover:bg-violet-500 rounded-full h-12 font-semibold glow-violet-sm"
                      data-testid="confirm-booking-btn"
                    >
                      {confirmMutation.isPending ? (
                        <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                      ) : null}
                      {t('confirmBooking')}
                    </Button>
                    <Button 
                      variant="ghost" 
                      onClick={() => {
                        setCurrentStep(2);
                        setHeldAppointment(null);
                        setExpiresAt(null);
                        setSelectedSlot(null);
                      }}
                      className="w-full text-slate-400"
                    >
                      {t('cancel')}
                    </Button>
                  </div>
                </Card>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </main>
    </div>
  );
}

export default BookingPage;
