import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';

export function RefereeConfirmResultsPage() {
  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 relative min-w-0 overflow-y-auto">
        <PageAmbience accent="red" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">

          <PageHero
            title="Xác nhận kết quả"
            subtitle="Xác nhận và công bố kết quả chính thức"
            imageUrl="/images/hero-referee.jpg"
            imagePosition="right 52%"
          />

          {/* TODO: BE chưa có API danh sách kết quả cuộc đua cần xác nhận */}
          <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
            <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
            <div className="text-4xl opacity-40 mb-3">🏁</div>
            <div className="text-muted text-sm">Chưa có dữ liệu</div>
          </div>

        </main>
      </div>
    </div>
  );
}
