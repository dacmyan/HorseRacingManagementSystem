import { useState, useEffect } from 'react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { getRefereeDashboard, submitResult } from '../../api/refereeService';
import { getRaceEntries } from '../../api/publicService';
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

  const [raceEntries, setRaceEntries] = useState<any[]>([]);
  const [loadingEntries, setLoadingEntries] = useState(false);

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

  async function handleRaceChange(raceId: string) {
    setF('raceId', raceId);
    setF('winner', '');
    setF('winningTime', '');
    setF('remarks', '');
    setError('');
    setRaceEntries([]);
    
    if (!raceId) return;

    setLoadingEntries(true);
    try {
      const res = await getRaceEntries(Number(raceId));
      const entries = res?.result ?? (Array.isArray(res) ? res : []);
      
      // Sort entries by FinishPosition, nulls at the end
      const sorted = [...entries].sort((a: any, b: any) => {
        if (a.finishPosition === null || a.finishPosition === undefined) return 1;
        if (b.finishPosition === null || b.finishPosition === undefined) return -1;
        return a.finishPosition - b.finishPosition;
      });
      setRaceEntries(sorted);

      const winnerEntry = sorted.find((e: any) => e.finishPosition === 1);
      if (winnerEntry) {
        setF('winner', winnerEntry.horseName || winnerEntry.horseId.toString());
        
        let formattedTime = '';
        if (winnerEntry.finishTime != null) {
          const totalSeconds = Number(winnerEntry.finishTime);
          const minutes = Math.floor(totalSeconds / 60);
          const seconds = (totalSeconds % 60).toFixed(2);
          const minutesStr = minutes.toString().padStart(2, '0');
          const secondsStr = seconds.padStart(5, '0'); // SS.SS
          formattedTime = `${minutesStr}:${secondsStr}`;
        }
        setF('winningTime', formattedTime);
        setF('remarks', 'Tự động ghi nhận từ kết quả cuộc đua hoàn thành.');
      } else {
        setError('Cuộc đua này chưa hoàn thành hoặc chưa có kết quả mô phỏng.');
      }
    } catch (err) {
      console.error(err);
      setError('Không thể tự động tải kết quả mô phỏng cho cuộc đua.');
    } finally {
      setLoadingEntries(false);
    }
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
      setRaceEntries([]);
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

          <div className={`grid grid-cols-1 ${form.raceId ? 'lg:grid-cols-2' : 'max-w-2xl'} gap-6 mx-auto transition-all duration-300`}>
            {/* Form Column */}
            <div className="glass-panel rounded-xl p-8 border border-glass-border h-fit">
              <h2 className="text-xl font-serif text-white mb-6">Nhập kết quả cuộc đua</h2>
              
              <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                  <label className={LABEL}>Trận đua *</label>
                  {loadingRaces ? (
                    <div className="text-sm text-muted">Đang tải...</div>
                  ) : (
                    <select value={form.raceId} onChange={e => handleRaceChange(e.target.value)} className={INPUT} style={{colorScheme: 'dark'}}>
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

            {/* Standings/Entries Column */}
            {form.raceId && (
              <div className="glass-panel rounded-xl p-8 border border-glass-border flex flex-col h-fit">
                <h2 className="text-xl font-serif text-white mb-6">Bảng xếp hạng lượt đua</h2>
                {loadingEntries ? (
                  <div className="text-sm text-muted py-6">Đang tải kết quả...</div>
                ) : raceEntries.length === 0 ? (
                  <div className="text-sm text-muted py-6 italic">Không có dữ liệu lượt đua</div>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                      <thead>
                        <tr className="border-b border-glass-border/30 text-[10px] text-muted uppercase">
                          <th className="py-2 pr-3">Hạng</th>
                          <th className="py-2 pr-3">Làn</th>
                          <th className="py-2 pr-3">Ngựa</th>
                          <th className="py-2 pr-3">Kỵ sĩ</th>
                          <th className="py-2 text-right">Thời gian</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-glass-border/20 text-white/90">
                        {raceEntries.map((entry: any) => (
                          <tr key={entry.raceEntryId}>
                            <td className="py-2 pr-3 font-bold text-gold">
                              {entry.finishPosition ? `${entry.finishPosition}` : '-'}
                            </td>
                            <td className="py-2 pr-3 text-muted">L{entry.laneNo}</td>
                            <td className="py-2 pr-3 font-medium">🐎 {entry.horseName}</td>
                            <td className="py-2 pr-3 text-muted">{entry.jockeyName || 'N/A'}</td>
                            <td className="py-2 text-right font-mono">
                              {entry.finishTime ? `${entry.finishTime}s` : '-'}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            )}
          </div>

        </main>
      </div>
    </div>
  );
}
