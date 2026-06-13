import { Search } from 'lucide-react';
import { useState } from 'react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';

export function OwnerTournamentsPage() {
  const [search, setSearch] = useState('');

  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 min-w-0 overflow-y-auto relative">
        <PageAmbience accent="emerald" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">

          <PageHero
            title="Giải đấu"
            subtitle="Các giải đấu đang và sắp diễn ra"
            imageUrl="/images/hero-owner.jpg"
            imagePosition="center 58%"
          />

          <div className="flex items-center gap-2 bg-white/[0.04] border border-glass-border focus-within:border-gold/40 rounded-lg px-3 py-2 w-64 transition-colors">
            <Search size={14} className="text-gold/60 shrink-0" />
            <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm giải đấu..." className="bg-transparent text-sm text-white placeholder:text-muted/60 outline-none w-full" />
          </div>

          {/* TODO: BE chưa có API danh sách giải đấu */}
          <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
            <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
            <div className="text-4xl opacity-40 mb-3">🏆</div>
            <div className="text-muted text-sm">Chưa có dữ liệu</div>
          </div>

        </main>
      </div>
    </div>
  );
}
