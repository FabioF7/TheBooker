import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Toaster } from './components/ui/sonner';
import { LanguageProvider } from './lib/i18n';
import BookingPage from './pages/BookingPage';
import AdminPage from './pages/AdminPage';
import './App.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60,
      retry: 1,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <LanguageProvider defaultLanguage="pt">
        <BrowserRouter>
          <div className="min-h-screen bg-slate-950">
            <Routes>
              <Route path="/" element={<BookingPage />} />
              <Route path="/admin" element={<AdminPage />} />
              <Route path="/:tenantSlug" element={<BookingPage />} />
            </Routes>
            <Toaster position="top-right" richColors />
          </div>
        </BrowserRouter>
      </LanguageProvider>
    </QueryClientProvider>
  );
}

export default App;
