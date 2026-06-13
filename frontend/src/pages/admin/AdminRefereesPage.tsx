import { motion } from 'framer-motion';
import { AlertCircle, CheckCircle } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';

export function AdminRefereesPage() {
  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 min-w-0 overflow-y-auto relative">
        <PageAmbience accent="gold" />
        <Topbar />
        <main className="max-w-[1600px] mx-auto px-8 py-6 space-y-6 relative z-10">

          <PageHero
            title="Referee Management"
            subtitle="Assign referees to races"
            imageUrl="/images/hero-admin.jpg"
            imagePosition="center center"
          />

          <div className="grid grid-cols-[1fr_320px] gap-6">
            {/* Left: Race Assignment */}
            <div className="space-y-4">
              {/* Needs Referee */}
              <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} className="glass-panel rounded-xl overflow-hidden">
                <div className="p-5 border-b border-glass-border flex items-center gap-2">
                  <AlertCircle size={16} className="text-yellow-400" />
                  <h2 className="text-base font-serif text-white">Unassigned</h2>
                  <span className="ml-auto px-2 py-0.5 rounded-full bg-yellow-500/10 text-yellow-400 text-[11px] font-bold border border-yellow-500/20">
                    0
                  </span>
                </div>
                {/* TODO: BE chưa có API danh sách cuộc đua cần trọng tài */}
                <div className="p-12 text-center relative overflow-hidden">
                  <div className="text-4xl opacity-40 mb-3">🧑‍⚖️</div>
                  <div className="text-muted text-sm">Chưa có dữ liệu</div>
                </div>
              </motion.div>

              {/* Already Assigned */}
              <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }} className="glass-panel rounded-xl overflow-hidden">
                <div className="p-5 border-b border-glass-border flex items-center gap-2">
                  <CheckCircle size={16} className="text-emerald-400" />
                  <h2 className="text-base font-serif text-white">Assigned</h2>
                  <span className="ml-auto px-2 py-0.5 rounded-full bg-emerald-500/10 text-emerald-400 text-[11px] font-bold border border-emerald-500/20">0</span>
                </div>
                {/* TODO: BE chưa có API danh sách cuộc đua đã phân công trọng tài */}
                <div className="p-12 text-center relative overflow-hidden">
                  <div className="text-4xl opacity-40 mb-3">🧑‍⚖️</div>
                  <div className="text-muted text-sm">Chưa có dữ liệu</div>
                </div>
              </motion.div>
            </div>

            {/* Right: Referee List */}
            <motion.div initial={{ opacity: 0, x: 16 }} animate={{ opacity: 1, x: 0 }} className="glass-panel rounded-xl p-5 h-fit">
              <h2 className="text-base font-serif text-white mb-4">Referee List</h2>
              {/* TODO: BE chưa có API danh sách trọng tài */}
              <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
                <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
                <div className="text-4xl opacity-40 mb-3">🧑‍⚖️</div>
                <div className="text-muted text-sm">Chưa có dữ liệu</div>
              </div>
            </motion.div>
          </div>

        </main>
      </div>
    </div>
  );
}
