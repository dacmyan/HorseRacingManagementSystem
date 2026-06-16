import { motion } from 'framer-motion';
import type { ReactNode } from 'react';

interface PageHeroProps {
  title: ReactNode;
  subtitle: string;
  imageUrl: string;
  imagePosition?: string;
  badge?: ReactNode;
  actions?: ReactNode;
}

export function PageHero({
  title,
  subtitle,
  imageUrl,
  imagePosition = 'right center',
  badge,
  actions,
}: PageHeroProps) {
  return (
    <div style={{
      padding: '1px',
      borderRadius: '16px',
      background: 'linear-gradient(135deg, rgba(212,175,55,0.45) 0%, rgba(212,175,55,0.06) 40%, rgba(212,175,55,0.35) 100%)',
      boxShadow: '0 0 30px rgba(212,175,55,0.10), 0 0 60px rgba(212,175,55,0.05)',
    }}>
    <motion.div
      initial={{ opacity: 0, y: -12 }}
      animate={{ opacity: 1, y: 0 }}
      className="rounded-2xl overflow-hidden relative"
      style={{
        minHeight: '150px',
        borderRadius: '15px',
        background: `linear-gradient(to right, rgba(11,18,36,0.98) 0%, rgba(11,18,36,0.75) 45%, rgba(11,18,36,0.2) 100%), url('${imageUrl}') ${imagePosition} / cover no-repeat`,
        boxShadow: 'inset 0 1px 0 rgba(212,175,55,0.12)',
      }}
    >
      {/* Corner ornaments */}
      <div className="absolute top-3 left-3 pointer-events-none z-20">
        <svg viewBox="0 0 30 30" fill="none" className="w-6 h-6">
          <path d="M0 30V0H30" stroke="#d4af37" strokeOpacity="0.5" strokeWidth="1"/>
          <circle cx="3" cy="3" r="1.2" fill="#d4af37" fillOpacity="0.7"/>
        </svg>
      </div>
      <div className="absolute top-3 right-3 pointer-events-none z-20" style={{transform:'scaleX(-1)'}}>
        <svg viewBox="0 0 30 30" fill="none" className="w-6 h-6">
          <path d="M0 30V0H30" stroke="#d4af37" strokeOpacity="0.5" strokeWidth="1"/>
          <circle cx="3" cy="3" r="1.2" fill="#d4af37" fillOpacity="0.7"/>
        </svg>
      </div>
      <div
        className="relative z-10 px-8 py-6 flex flex-col items-start justify-center"
        style={{ minHeight: '150px' }}
      >
        {badge && <div className="mb-2">{badge}</div>}
        <h1 className="text-2xl font-serif text-white mb-1">{title}</h1>
        <p className="text-sm text-muted">{subtitle}</p>
        {actions && <div className="flex gap-3 mt-4">{actions}</div>}
      </div>
    </motion.div>
    </div>
  );
}
