import React, { createContext, useContext, useState, useEffect } from 'react';
import translations from './translations';

const LanguageContext = createContext();

export function LanguageProvider({ children, defaultLanguage = 'en' }) {
  const [language, setLanguage] = useState(defaultLanguage);

  const t = (key) => {
    return translations[language]?.[key] || translations['en']?.[key] || key;
  };

  const toggleLanguage = () => {
    setLanguage(prev => prev === 'en' ? 'pt' : 'en');
  };

  return (
    <LanguageContext.Provider value={{ language, setLanguage, toggleLanguage, t }}>
      {children}
    </LanguageContext.Provider>
  );
}

export function useLanguage() {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error('useLanguage must be used within LanguageProvider');
  }
  return context;
}

export function LanguageToggle({ className = '' }) {
  const { language, toggleLanguage } = useLanguage();
  
  return (
    <button
      onClick={toggleLanguage}
      className={`flex items-center gap-2 px-3 py-1.5 rounded-full bg-slate-800/50 hover:bg-slate-800 border border-slate-700 transition-all text-sm font-medium ${className}`}
      data-testid="language-toggle"
    >
      <span className={language === 'en' ? 'text-violet-400' : 'text-slate-400'}>EN</span>
      <span className="text-slate-600">/</span>
      <span className={language === 'pt' ? 'text-violet-400' : 'text-slate-400'}>PT</span>
    </button>
  );
}
