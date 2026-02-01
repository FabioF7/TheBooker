import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Calendar, Clock, User, ChevronRight, Loader2, Check } from 'lucide-react';
import { format, addDays, startOfToday } from 'date-fns';
import { toast } from 'sonner';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Calendar as CalendarComponent } from '../components/ui/calendar';
import { 
  useTenants, 
  useTenantServices, 
  useTenantProviders, 
  useAvailability,
  useHoldSlot,
  useConfirmAppointment 
} from '../hooks/useBooking';

const STEPS = ['service', 'provider', 'datetime', 'confirm'];

function BookingPage() {
  const { tenantSlug } = useParams();
  const [currentStep, setCurrentStep] = useState(0);
  const [sessionId] = useState(() => crypto.randomUUID());
  
  // Selection state
  const [selectedTenant, setSelectedTenant] = useState(null);
  const [selectedService, setSelectedService] = useState(null);
  const [selectedProvider, setSelectedProvider] = useState(null);
  const [selectedDate, setSelectedDate] = useState(startOfToday());
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
  const { data: tenants, isLoading: tenantsLoading } = useTenants();
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

  // Auto-select tenant if slug provided
  useEffect(() => {
    if (tenants && tenantSlug) {
      const tenant = tenants.find(t => t.slug === tenantSlug);
      if (tenant) setSelectedTenant(tenant);
    } else if (tenants && tenants.length === 1) {
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
      setCurrentStep(3); // Move to confirm step
      toast.success('Slot held for 10 minutes');
    } catch (error) {
      toast.error(error.response?.data?.detail || 'Failed to hold slot');
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
      toast.success('Booking confirmed!');
    } catch (error) {
      toast.error(error.response?.data?.detail || 'Failed to confirm booking');
    }
  };

  // Countdown timer for held slot
  const [timeRemaining, setTimeRemaining] = useState(null);
  
  useEffect(() => {
    if (!expiresAt) return;
    
    const interval = setInterval(() => {
      const remaining = Math.max(0, Math.floor((expiresAt - new Date()) / 1000));
      setTimeRemaining(remaining);
      
      if (remaining === 0) {
        toast.error('Slot hold expired');
        setHeldAppointment(null);
        setExpiresAt(null);
        setSelectedSlot(null);
        setCurrentStep(2);
        refetchAvailability();
      }
    }, 1000);
    
    return () => clearInterval(interval);
  }, [expiresAt, refetchAvailability]);

  // Booking complete view
  if (bookingComplete && confirmedBooking) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <Card className="w-full max-w-md bg-slate-900 border-slate-800" data-testid="booking-confirmed-card">
          <CardHeader className="text-center">
            <div className="mx-auto w-16 h-16 rounded-full bg-green-500/20 flex items-center justify-center mb-4">
              <Check className="w-8 h-8 text-green-500" />
            </div>
            <CardTitle className="text-2xl text-white">Booking Confirmed!</CardTitle>
            <CardDescription className="text-slate-400">Your appointment has been scheduled</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="bg-slate-800/50 rounded-lg p-4 space-y-2">
              <div className="flex justify-between">
                <span className="text-slate-400">Service</span>
                <span className="text-white font-medium">{selectedService?.name}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Provider</span>
                <span className="text-white font-medium">{selectedProvider?.name}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Date</span>
                <span className="text-white font-medium">{format(selectedDate, 'MMMM d, yyyy')}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Time</span>
                <span className="text-white font-medium">{selectedSlot?.startTime} - {selectedSlot?.endTime}</span>
              </div>
            </div>
            <p className="text-sm text-slate-500 text-center">
              A confirmation email has been sent to {customerEmail}
            </p>
            <Button 
              className="w-full" 
              onClick={() => window.location.reload()}
              data-testid="book-another-btn"
            >
              Book Another Appointment
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-white mb-2">
            {selectedTenant?.name || 'The Booker'}
          </h1>
          <p className="text-slate-400">Schedule your appointment</p>
        </div>

        {/* Progress Steps */}
        <div className="flex justify-center mb-8">
          <div className="flex items-center space-x-2">
            {STEPS.map((step, index) => (
              <React.Fragment key={step}>
                <div 
                  className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium transition-colors
                    ${index <= currentStep 
                      ? 'bg-blue-600 text-white' 
                      : 'bg-slate-800 text-slate-500'}`}
                  data-testid={`step-${step}`}
                >
                  {index + 1}
                </div>
                {index < STEPS.length - 1 && (
                  <ChevronRight className={`w-4 h-4 ${index < currentStep ? 'text-blue-600' : 'text-slate-700'}`} />
                )}
              </React.Fragment>
            ))}
          </div>
        </div>

        {/* Step Content */}
        <div className="grid gap-6">
          {/* Step 1: Select Service */}
          {currentStep === 0 && (
            <Card className="bg-slate-900 border-slate-800" data-testid="service-selection">
              <CardHeader>
                <CardTitle className="text-white flex items-center gap-2">
                  <Calendar className="w-5 h-5" />
                  Select a Service
                </CardTitle>
              </CardHeader>
              <CardContent>
                {servicesLoading ? (
                  <div className="flex justify-center py-8">
                    <Loader2 className="w-6 h-6 animate-spin text-blue-500" />
                  </div>
                ) : services?.length === 0 ? (
                  <p className="text-slate-500 text-center py-8">No services available</p>
                ) : (
                  <div className="grid gap-3">
                    {services?.map(service => (
                      <button
                        key={service.id}
                        onClick={() => {
                          setSelectedService(service);
                          setCurrentStep(1);
                        }}
                        className="flex items-center justify-between p-4 rounded-lg bg-slate-800/50 hover:bg-slate-800 transition-colors text-left"
                        data-testid={`service-${service.id}`}
                      >
                        <div>
                          <h3 className="font-medium text-white">{service.name}</h3>
                          <p className="text-sm text-slate-400">
                            {service.durationMinutes} min • ${service.price?.amount || 0}
                          </p>
                        </div>
                        <ChevronRight className="w-5 h-5 text-slate-500" />
                      </button>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* Step 2: Select Provider */}
          {currentStep === 1 && (
            <Card className="bg-slate-900 border-slate-800" data-testid="provider-selection">
              <CardHeader>
                <CardTitle className="text-white flex items-center gap-2">
                  <User className="w-5 h-5" />
                  Select a Provider
                </CardTitle>
                <CardDescription className="text-slate-400">
                  For: {selectedService?.name}
                </CardDescription>
              </CardHeader>
              <CardContent>
                {providersLoading ? (
                  <div className="flex justify-center py-8">
                    <Loader2 className="w-6 h-6 animate-spin text-blue-500" />
                  </div>
                ) : availableProviders.length === 0 ? (
                  <p className="text-slate-500 text-center py-8">No providers available for this service</p>
                ) : (
                  <div className="grid gap-3">
                    {availableProviders.map(provider => (
                      <button
                        key={provider.id}
                        onClick={() => {
                          setSelectedProvider(provider);
                          setCurrentStep(2);
                        }}
                        className="flex items-center justify-between p-4 rounded-lg bg-slate-800/50 hover:bg-slate-800 transition-colors text-left"
                        data-testid={`provider-${provider.id}`}
                      >
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-full bg-blue-600 flex items-center justify-center">
                            <span className="text-white font-medium">
                              {provider.name.charAt(0).toUpperCase()}
                            </span>
                          </div>
                          <div>
                            <h3 className="font-medium text-white">{provider.name}</h3>
                            {provider.email && (
                              <p className="text-sm text-slate-400">{provider.email}</p>
                            )}
                          </div>
                        </div>
                        <ChevronRight className="w-5 h-5 text-slate-500" />
                      </button>
                    ))}
                  </div>
                )}
                <Button 
                  variant="ghost" 
                  className="mt-4" 
                  onClick={() => setCurrentStep(0)}
                >
                  Back
                </Button>
              </CardContent>
            </Card>
          )}

          {/* Step 3: Select Date & Time */}
          {currentStep === 2 && (
            <div className="grid md:grid-cols-2 gap-6">
              <Card className="bg-slate-900 border-slate-800" data-testid="date-selection">
                <CardHeader>
                  <CardTitle className="text-white">Select Date</CardTitle>
                </CardHeader>
                <CardContent>
                  <CalendarComponent
                    mode="single"
                    selected={selectedDate}
                    onSelect={setSelectedDate}
                    disabled={(date) => date < startOfToday()}
                    className="rounded-md border border-slate-800"
                  />
                  <Button 
                    variant="ghost" 
                    className="mt-4" 
                    onClick={() => setCurrentStep(1)}
                  >
                    Back
                  </Button>
                </CardContent>
              </Card>

              <Card className="bg-slate-900 border-slate-800" data-testid="time-selection">
                <CardHeader>
                  <CardTitle className="text-white flex items-center gap-2">
                    <Clock className="w-5 h-5" />
                    Available Times
                  </CardTitle>
                  <CardDescription className="text-slate-400">
                    {selectedDate && format(selectedDate, 'EEEE, MMMM d, yyyy')}
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  {availabilityLoading ? (
                    <div className="flex justify-center py-8">
                      <Loader2 className="w-6 h-6 animate-spin text-blue-500" />
                    </div>
                  ) : !availability?.isOpen ? (
                    <p className="text-slate-500 text-center py-8">
                      {availability?.closedReason || 'Closed on this day'}
                    </p>
                  ) : availability?.slots?.length === 0 ? (
                    <p className="text-slate-500 text-center py-8">No available slots</p>
                  ) : (
                    <div className="grid grid-cols-3 gap-2 max-h-80 overflow-y-auto">
                      {availability?.slots?.map((slot, index) => (
                        <button
                          key={index}
                          onClick={() => handleSlotSelect(slot)}
                          disabled={!slot.isAvailable || holdSlotMutation.isPending}
                          className={`p-2 text-sm rounded-lg transition-colors
                            ${slot.isAvailable 
                              ? 'bg-slate-800 hover:bg-blue-600 text-white' 
                              : 'bg-slate-800/30 text-slate-600 cursor-not-allowed'}`}
                          data-testid={`slot-${slot.startTime}`}
                        >
                          {slot.startTime}
                        </button>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            </div>
          )}

          {/* Step 4: Confirm & Customer Info */}
          {currentStep === 3 && heldAppointment && (
            <Card className="bg-slate-900 border-slate-800" data-testid="confirm-booking">
              <CardHeader>
                <CardTitle className="text-white">Confirm Your Booking</CardTitle>
                {timeRemaining !== null && (
                  <CardDescription className={`${timeRemaining < 60 ? 'text-red-400' : 'text-slate-400'}`}>
                    Complete within: {Math.floor(timeRemaining / 60)}:{String(timeRemaining % 60).padStart(2, '0')}
                  </CardDescription>
                )}
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Booking Summary */}
                <div className="bg-slate-800/50 rounded-lg p-4 space-y-2">
                  <div className="flex justify-between">
                    <span className="text-slate-400">Service</span>
                    <span className="text-white">{selectedService?.name}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">Provider</span>
                    <span className="text-white">{selectedProvider?.name}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">Date</span>
                    <span className="text-white">{format(selectedDate, 'MMMM d, yyyy')}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">Time</span>
                    <span className="text-white">{selectedSlot?.startTime} - {selectedSlot?.endTime}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">Duration</span>
                    <span className="text-white">{selectedService?.durationMinutes} minutes</span>
                  </div>
                  <div className="flex justify-between border-t border-slate-700 pt-2 mt-2">
                    <span className="text-slate-400">Price</span>
                    <span className="text-white font-medium">${selectedService?.price?.amount || 0}</span>
                  </div>
                </div>

                {/* Customer Form */}
                <div className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="name" className="text-slate-300">Full Name *</Label>
                    <Input
                      id="name"
                      value={customerName}
                      onChange={(e) => setCustomerName(e.target.value)}
                      placeholder="John Doe"
                      className="bg-slate-800 border-slate-700"
                      data-testid="customer-name-input"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="email" className="text-slate-300">Email *</Label>
                    <Input
                      id="email"
                      type="email"
                      value={customerEmail}
                      onChange={(e) => setCustomerEmail(e.target.value)}
                      placeholder="john@example.com"
                      className="bg-slate-800 border-slate-700"
                      data-testid="customer-email-input"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="phone" className="text-slate-300">Phone (optional)</Label>
                    <Input
                      id="phone"
                      type="tel"
                      value={customerPhone}
                      onChange={(e) => setCustomerPhone(e.target.value)}
                      placeholder="+1 234 567 8900"
                      className="bg-slate-800 border-slate-700"
                      data-testid="customer-phone-input"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="notes" className="text-slate-300">Notes (optional)</Label>
                    <Input
                      id="notes"
                      value={notes}
                      onChange={(e) => setNotes(e.target.value)}
                      placeholder="Any special requests..."
                      className="bg-slate-800 border-slate-700"
                      data-testid="customer-notes-input"
                    />
                  </div>
                </div>

                <div className="flex gap-3">
                  <Button 
                    variant="outline" 
                    onClick={() => {
                      setCurrentStep(2);
                      setHeldAppointment(null);
                      setExpiresAt(null);
                      setSelectedSlot(null);
                    }}
                    className="flex-1"
                  >
                    Cancel
                  </Button>
                  <Button 
                    onClick={handleConfirm}
                    disabled={!customerName || !customerEmail || confirmMutation.isPending}
                    className="flex-1"
                    data-testid="confirm-booking-btn"
                  >
                    {confirmMutation.isPending ? (
                      <Loader2 className="w-4 h-4 animate-spin mr-2" />
                    ) : null}
                    Confirm Booking
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

export default BookingPage;
