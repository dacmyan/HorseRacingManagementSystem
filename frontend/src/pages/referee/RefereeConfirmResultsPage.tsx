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

  const selectedRace = races.find(r => r.raceId === Number(form.raceId));
  const isCompleted = selectedRace && (selectedRace.status === 'Completed' || selectedRace.status === 'Finished');

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

      // If the race doesn't have results yet, pre-populate default simulated results
      const hasResults = sorted.some((e: any) => e.finishPosition != null);
      if (!hasResults) {
        const baseWinnerTime = Math.round(55 + Math.random() * 10);
        const participatingEntries = sorted.filter((e: any) =>
          !['Withdrawn', 'Scratch', 'DNF', 'Disqualified'].includes(e.status) &&
          e.healthStatus !== 'Sick' && e.healthStatus !== 'Injured'
        );

        let position = 1;
        sorted.forEach((e: any) => {
          const isParticipating = participatingEntries.includes(e);
          if (isParticipating) {
            const index = participatingEntries.indexOf(e);
            e.finishPosition = position++;
            e.finishTime = index === 0
              ? baseWinnerTime
              : Number((baseWinnerTime + (index * 1.5) + Math.random() * 2).toFixed(2));
          } else {
            e.finishPosition = null;
            e.finishTime = null;
          }
        });
      }

      setRaceEntries(sorted);

      const winnerEntry = sorted.find((e: any) => e.finishPosition === 1);
      if (winnerEntry) {
        setF('winner', winnerEntry.horseName || winnerEntry.horseId.toString());
        setF('winningTime', String(winnerEntry.finishTime));
        setF('remarks', 'Đã ghi nhận kết quả.');
      } else {
        setF('remarks', 'Trọng tài ghi nhận kết quả thủ công.');
      }
    } catch (err) {
      console.error(err);
      setError('Không thể tải danh sách ngựa cho cuộc đua.');
    } finally {
      setLoadingEntries(false);
    }
  }

  function handleEntryChange(raceEntryId: number, field: string, value: string) {
    setRaceEntries(prev => prev.map(entry => {
      if (entry.raceEntryId === raceEntryId) {
        const val = value === '' ? null : Number(value);
        return { ...entry, [field]: val };
      }
      return entry;
    }));
  }

  useEffect(() => {
    const winnerEntry = raceEntries.find(e => Number(e.finishPosition) === 1);
    if (winnerEntry) {
      setForm(p => ({
        ...p,
        winner: winnerEntry.horseName || String(winnerEntry.horseId),
        winningTime: winnerEntry.finishTime != null ? String(winnerEntry.finishTime) : ''
      }));
    }
  }, [raceEntries]);

  useEffect(() => {
    if (form.raceId) {
      const race = races.find(r => r.raceId === Number(form.raceId));
      if (race && (race.status === 'Completed' || race.status === 'Finished')) {
        setError('A result has already been submitted for this race.');
      } else {
        setError('');
      }
    } else {
      setError('');
    }
  }, [form.raceId, races]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(''); setSuccess('');
    if (!form.raceId) {
      setError('Vui lòng chọn trận đua.');
      return;
    }

    if (isCompleted) {
      setError('A result has already been submitted for this race.');
      return;
    }
    
    const invalidEntry = raceEntries.find(e =>
      !['Withdrawn', 'Scratch', 'DNF', 'Disqualified'].includes(e.status) &&
      e.healthStatus !== 'Sick' &&
      e.healthStatus !== 'Injured' &&
      (e.finishPosition == null || e.finishTime == null)
    );
    if (invalidEntry) {
      setError('Vui lòng nhập đầy đủ hạng và thời gian cho tất cả ngựa thi đấu.');
      return;
    }

    const winners = raceEntries.filter(e => Number(e.finishPosition) === 1);
    if (winners.length !== 1) {
      setError('Vui lòng chọn duy nhất một ngựa đạt Hạng 1.');
      return;
    }

    const winnerEntry = winners[0];
    if (winnerEntry && (['Withdrawn', 'Scratch', 'DNF', 'Disqualified'].includes(winnerEntry.status) ||
                       winnerEntry.healthStatus === 'Sick' ||
                       winnerEntry.healthStatus === 'Injured')) {
      setError('Ngựa chiến thắng không thể là ngựa bị bệnh hoặc đã rút lui.');
      return;
    }

    if (!form.winner || !form.winningTime) {
      setError('Thiếu thông tin người chiến thắng.');
      return;
    }

    setLoading(true);
    try {
      await submitResult({
        raceId: Number(form.raceId),
        winner: form.winner,
        winningTime: form.winningTime,
        remarks: form.remarks,
        entries: raceEntries.map(e => {
          const isSickOrWithdrawn = ['Withdrawn', 'Scratch', 'DNF', 'Disqualified'].includes(e.status) ||
            e.healthStatus === 'Sick' || e.healthStatus === 'Injured';
          return {
            raceEntryId: e.raceEntryId,
            finishPosition: isSickOrWithdrawn ? 0 : Number(e.finishPosition),
            finishTime: isSickOrWithdrawn ? 0 : Number(e.finishTime)
          };
        })
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
                  <input disabled={!!isCompleted} value={form.winner} onChange={e => setF('winner', e.target.value)} placeholder="Tên ngựa hoặc ID ngựa" className={INPUT} />
                </div>
                
                <div>
                  <label className={LABEL}>Thời gian hoàn thành (Winning Time) *</label>
                  <input disabled={!!isCompleted} value={form.winningTime} onChange={e => setF('winningTime', e.target.value)} placeholder="VD: 01:23.45" className={INPUT} />
                </div>
                
                <div>
                  <label className={LABEL}>Ghi chú thêm</label>
                  <textarea disabled={!!isCompleted} rows={3} value={form.remarks} onChange={e => setF('remarks', e.target.value)} placeholder="Ví dụ: Về đích sát sao, kỷ lục mới..." className={INPUT + " resize-none"} />
                </div>

                {error && <div className="text-sm px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400">{error}</div>}
                {success && <div className="text-sm px-4 py-3 rounded-lg bg-emerald-500/10 border border-emerald-500/20 text-emerald-400">{success}</div>}

                <div className="pt-2">
                  <button type="submit" disabled={loading || !!isCompleted} className="btn-red w-full py-3 rounded-lg font-bold disabled:opacity-60 disabled:cursor-not-allowed">
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
                          <th className="py-2 pr-3">Làn</th>
                          <th className="py-2 pr-3">Ngựa</th>
                          <th className="py-2 pr-3">Kỵ sĩ</th>
                          <th className="py-2 pr-3 w-28">Hạng (Position)</th>
                          <th className="py-2 text-right w-36">Thời gian (Giây)</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-glass-border/20 text-white/90">
                        {raceEntries.map((entry: any) => {
                          const isSick = entry.healthStatus === 'Sick' || entry.healthStatus === 'Injured';
                          const isWithdrawn = ['Withdrawn', 'Scratch', 'DNF', 'Disqualified'].includes(entry.status);
                          const isDisabled = isSick || isWithdrawn;
                          return (
                            <tr key={entry.raceEntryId} className={isDisabled ? 'opacity-60 bg-white/[0.02]' : ''}>
                              <td className="py-3 pr-3 text-muted">L{entry.laneNo}</td>
                              <td className="py-3 pr-3 font-medium">
                                <div>🐎 {entry.horseName}</div>
                                {isSick && (
                                  <span className="inline-block text-[10px] bg-red-500/20 text-red-400 border border-red-500/30 rounded-full px-2 py-0.5 mt-1 font-sans">
                                    {entry.healthStatus === 'Sick' ? 'Bị bệnh' : 'Chấn thương'}
                                  </span>
                                )}
                                {isWithdrawn && (
                                  <span className="inline-block text-[10px] bg-amber-500/20 text-amber-400 border border-amber-500/30 rounded-full px-2 py-0.5 mt-1 ml-1 font-sans">
                                    {entry.status === 'Withdrawn' ? 'Đã rút' : entry.status === 'DNF' ? 'Không hoàn thành' : entry.status}
                                  </span>
                                )}
                              </td>
                              <td className="py-3 pr-3 text-muted">{entry.jockeyName || 'N/A'}</td>
                              <td className="py-3 pr-3">
                                <input 
                                  disabled={!!isCompleted || isDisabled}
                                  type="number" 
                                  min="1"
                                  max={raceEntries.length}
                                  value={entry.finishPosition ?? ''} 
                                  onChange={e => handleEntryChange(entry.raceEntryId, 'finishPosition', e.target.value)}
                                  placeholder={isDisabled ? 'N/A' : 'VD: 1'} 
                                  className="w-16 bg-navy/40 border border-glass-border/60 rounded px-2 py-1 text-sm text-white text-center focus:border-red-400/40 outline-none disabled:opacity-40 disabled:border-glass-border/20"
                                />
                              </td>
                              <td className="py-3 text-right">
                                <input 
                                  disabled={!!isCompleted || isDisabled}
                                  type="number" 
                                  step="0.01" 
                                  min="0"
                                  value={entry.finishTime ?? ''} 
                                  onChange={e => handleEntryChange(entry.raceEntryId, 'finishTime', e.target.value)}
                                  placeholder={isDisabled ? 'N/A' : 'VD: 60.55'} 
                                  className="w-28 bg-navy/40 border border-glass-border/60 rounded px-2 py-1 text-sm text-white text-right focus:border-red-400/40 outline-none disabled:opacity-40 disabled:border-glass-border/20"
                                />
                              </td>
                            </tr>
                          );
                        })}
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
