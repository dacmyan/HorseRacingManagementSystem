import { useState, useEffect } from 'react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { getRefereeDashboard, submitResult } from '../../api/refereeService';
import { parseApiError } from '../../api/authService';

const INPUT = 'w-full bg-navy/50 border border-glass-border rounded-lg px-4 py-2.5 text-sm text-white placeholder:text-muted/60 outline-none focus:border-red-400/40 transition-colors';
const LABEL = 'block text-xs font-bold text-muted uppercase tracking-wider mb-1.5';

export function RefereeConfirmResultsPage() {
  const [races, setRaces] = useState<any[]>([]);
  const [loadingRaces, setLoadingRaces] = useState(true);

  const [form, setForm] = useState({ raceId: '', winner: '', winningTime: '', remarks: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    getRefereeDashboard()
      .then((data: any) => {
        const list = data?.result?.assignedRaces || [];
        setRaces(list);
      })
      .catch(() => setRaces([]))
      .finally(() => setLoadingRaces(false));
  }, []);

  function setF(field: string, val: string) {
    setForm(p => ({ ...p, [field]: val }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(''); setSuccess('');
    if (!form.raceId || !form.winner || !form.winningTime) {
      setError('Vui lòng điền đủ Race, Winner và Time.');
      return;
    }
    setLoading(true);
    try {
      await submitResult({
        raceId: Number(form.raceId),
        winner: form.winner,
        winningTime: form.winningTime,
        remarks: form.remarks
      });
      setSuccess('Ghi nhận kết quả thành công!');
      setForm({ raceId: '', winner: '', winningTime: '', remarks: '' });
    } catch (err: unknown) {
      setError(parseApiError(err as Error));
    } finally {
      setLoading(false);
    }
  }

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

          <div className="glass-panel rounded-xl p-8 max-w-2xl mx-auto border border-glass-border">
            <h2 className="text-xl font-serif text-white mb-6">Nhập kết quả cuộc đua</h2>
            
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className={LABEL}>Trận đua *</label>
                {loadingRaces ? (
                  <div className="text-sm text-muted">Đang tải...</div>
                ) : (
                  <select value={form.raceId} onChange={e => setF('raceId', e.target.value)} className={INPUT} style={{colorScheme: 'dark'}}>
                    <option value="">-- Chọn trận đua --</option>
                    {races.map(r => (
                      <option key={r.raceId} value={r.raceId}>ID {r.raceId}: {r.raceName} ({r.status})</option>
                    ))}
                  </select>
                )}
              </div>
              
              <div>
                <label className={LABEL}>Ngựa chiến thắng *</label>
                <input value={form.winner} onChange={e => setF('winner', e.target.value)} placeholder="Tên ngựa hoặc ID ngựa" className={INPUT} />
              </div>
              
              <div>
                <label className={LABEL}>Thời gian hoàn thành (Winning Time) *</label>
                <input value={form.winningTime} onChange={e => setF('winningTime', e.target.value)} placeholder="VD: 01:23.45" className={INPUT} />
              </div>
              
              <div>
                <label className={LABEL}>Ghi chú thêm</label>
                <textarea rows={3} value={form.remarks} onChange={e => setF('remarks', e.target.value)} placeholder="Ví dụ: Về đích sát sao, kỷ lục mới..." className={INPUT + " resize-none"} />
              </div>

              {error && <div className="text-sm px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400">{error}</div>}
              {success && <div className="text-sm px-4 py-3 rounded-lg bg-emerald-500/10 border border-emerald-500/20 text-emerald-400">{success}</div>}

              <div className="pt-2">
                <button type="submit" disabled={loading} className="btn-red w-full py-3 rounded-lg font-bold disabled:opacity-60 disabled:cursor-not-allowed">
                  {loading ? 'Đang gửi...' : 'Xác nhận kết quả'}
                </button>
              </div>
            </form>
          </div>

        </main>
      </div>
    </div>
  );
}
