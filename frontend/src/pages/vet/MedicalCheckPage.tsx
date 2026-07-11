import { useState, useEffect } from 'react';
import { Search, Plus, Edit2, Trash2, RefreshCw } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { 
  getMedicalChecks, 
  createMedicalCheck, 
  updateMedicalCheck, 
  deleteMedicalCheck, 
  getPendingRegistrations,
  getAssignedEntries,
  performRecheck
} from '../../api/vetService';

type Tab = 'pending' | 'assigned' | 'history';

interface PendingCheck {
  registrationId: number;
  horseName: string;
  tournamentName: string;
  ownerName: string;
  registeredAt: string;
}

interface AssignedEntry {
  raceEntryId: number;
  raceId: number;
  raceName: string | null;
  raceDate: string;
  raceStatus: string;
  laneNo: number;
  raceEntryStatus: string;
  registrationId: number;
  horseName: string | null;
  ownerName: string | null;
  jockeyName: string | null;
  tournamentName: string | null;
  lastMedicalResult: string | null;
  lastCheckType: string | null;
  lastCheckedAt: string | null;
}

interface MedicalRecord {
  id: number;
  registrationId: number;
  horseName: string;
  tournamentName: string;
  userId: number;
  checkedByName: string;
  checkType: string;
  weight: number;
  temperature: number | null;
  heartRate: number | null;
  dopingResult: string;
  medicalResult: string;
  failReason: string | null;
  notes: string | null;
  checkedAt: string;
}

export function MedicalCheckPage() {
  const [activeTab, setActiveTab] = useState<Tab>('pending');
  const [search, setSearch] = useState('');
  
  const [pendingList, setPendingList] = useState<PendingCheck[]>([]);
  const [assignedList, setAssignedList] = useState<AssignedEntry[]>([]);
  const [historyList, setHistoryList] = useState<MedicalRecord[]>([]);
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Modals state
  const [showModal, setShowModal] = useState(false);
  const [modalType, setModalType] = useState<'create' | 'edit' | 'recheck'>('create');
  const [selectedRecordId, setSelectedRecordId] = useState<number | null>(null);
  const [selectedRegId, setSelectedRegId] = useState<number | null>(null);
  const [selectedHorseName, setSelectedHorseName] = useState('');
  
  // Form fields
  const [weight, setWeight] = useState<string>('');
  const [temperature, setTemperature] = useState<string>('');
  const [heartRate, setHeartRate] = useState<string>('');
  const [dopingResult, setDopingResult] = useState<string>('Negative');
  const [medicalResult, setMedicalResult] = useState<string>('Pass');
  const [failReason, setFailReason] = useState<string>('FailedMedicalReCheck');
  const [notes, setNotes] = useState<string>('');

  useEffect(() => {
    loadData();
  }, [activeTab]);

  const loadData = () => {
    setLoading(true);
    setError('');
    
    if (activeTab === 'pending') {
      getPendingRegistrations()
        .then(res => {
          if (res && res.result) {
            setPendingList(res.result);
          }
          setLoading(false);
        })
        .catch(err => {
          console.error(err);
          setError('Không thể tải danh sách ngựa cần kiểm tra.');
          setLoading(false);
        });
    } else if (activeTab === 'assigned') {
      getAssignedEntries()
        .then(res => {
          if (res && res.result) {
            setAssignedList(res.result);
          }
          setLoading(false);
        })
        .catch(err => {
          console.error(err);
          setError('Không thể tải danh sách ngựa đã xếp lịch đua.');
          setLoading(false);
        });
    } else {
      getMedicalChecks()
        .then(res => {
          if (res && res.result) {
            setHistoryList(res.result);
          }
          setLoading(false);
        })
        .catch(err => {
          console.error(err);
          setError('Không thể tải lịch sử kiểm tra.');
          setLoading(false);
        });
    }
  };

  const handleOpenCreateModal = (pc: PendingCheck) => {
    setModalType('create');
    setSelectedRegId(pc.registrationId);
    setSelectedHorseName(pc.horseName);
    setWeight('');
    setTemperature('');
    setHeartRate('');
    setDopingResult('Negative');
    setMedicalResult('Pass');
    setFailReason('');
    setNotes('');
    setShowModal(true);
  };

  const handleOpenRecheckModal = (ae: AssignedEntry) => {
    setModalType('recheck');
    setSelectedRegId(ae.registrationId);
    setSelectedHorseName(ae.horseName || '');
    setWeight('');
    setTemperature('');
    setHeartRate('');
    setDopingResult('Negative');
    setMedicalResult('Pass');
    setFailReason('FailedMedicalReCheck');
    setNotes('');
    setShowModal(true);
  };

  const handleOpenEditModal = (mr: MedicalRecord) => {
    setModalType('edit');
    setSelectedRecordId(mr.id);
    setSelectedHorseName(mr.horseName);
    setWeight(mr.weight.toString());
    setTemperature(mr.temperature ? mr.temperature.toString() : '');
    setHeartRate(mr.heartRate ? mr.heartRate.toString() : '');
    setDopingResult(mr.dopingResult);
    setMedicalResult(mr.medicalResult);
    setFailReason(mr.failReason || '');
    setNotes(mr.notes || '');
    setShowModal(true);
  };

  const handleDeleteRecord = (id: number) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa bệnh án kiểm tra này?')) return;
    
    setLoading(true);
    deleteMedicalCheck(id)
      .then(() => {
        setSuccess('Đã xóa bệnh án kiểm tra thành công!');
        loadData();
      })
      .catch(err => {
        console.error(err);
        setError('Lỗi khi xóa bệnh án kiểm tra.');
        setLoading(false);
      });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!weight || parseFloat(weight) <= 0) {
      setError('Cân nặng phải lớn hơn 0.');
      return;
    }

    if (medicalResult === 'Fail' && !failReason && modalType !== 'edit') {
      setError('Vui lòng chọn lý do không đạt chuẩn y tế.');
      return;
    }

    setLoading(true);
    setError('');
    setSuccess('');

    if (modalType === 'recheck') {
      if (medicalResult === 'Fail') {
        const confirmWithdraw = window.confirm(
          `CẢNH BÁO: Việc đánh giá ngựa '${selectedHorseName}' KHÔNG ĐẠT CHUẨN sẽ tự động loại ngựa này và rút (Withdrawn/DNF) khỏi cuộc đua hiện tại. Bạn có chắc chắn muốn tiếp tục?`
        );
        if (!confirmWithdraw) {
          setLoading(false);
          return;
        }
      }

      const recheckData = {
        registrationId: selectedRegId,
        weight: parseFloat(weight),
        temperature: temperature ? parseFloat(temperature) : null,
        heartRate: heartRate ? parseInt(heartRate) : null,
        dopingResult,
        medicalResult,
        failReason: medicalResult === 'Fail' ? failReason : null,
        notes: notes || null
      };

      performRecheck(recheckData)
        .then((res) => {
          setSuccess(res.message || 'Đã lưu kết quả tái khám thành công!');
          setShowModal(false);
          loadData();
        })
        .catch(err => {
          console.error(err);
          const msg = err.response?.data?.message || 'Có lỗi xảy ra khi thực hiện tái khám.';
          setError(msg);
          setLoading(false);
        });
      return;
    }

    const data = {
      registrationId: selectedRegId,
      weight: parseFloat(weight),
      temperature: temperature ? parseFloat(temperature) : null,
      heartRate: heartRate ? parseInt(heartRate) : null,
      dopingResult,
      medicalResult,
      failReason: medicalResult === 'Fail' ? failReason : null,
      notes: notes || null
    };

    if (modalType === 'create') {
      createMedicalCheck(data)
        .then(() => {
          setSuccess('Đã lưu kết quả kiểm tra thành công!');
          setShowModal(false);
          loadData();
        })
        .catch(err => {
          console.error(err);
          const msg = err.response?.data?.message || 'Có lỗi xảy ra khi tạo kết quả kiểm tra.';
          setError(msg);
          setLoading(false);
        });

    } else {
      updateMedicalCheck(selectedRecordId!, data)
        .then(() => {
          setSuccess('Đã cập nhật kết quả kiểm tra thành công!');
          setShowModal(false);
          loadData();
        })
        .catch(err => {
          console.error(err);
          const msg = err.response?.data?.message || 'Có lỗi xảy ra khi cập nhật kết quả kiểm tra.';
          setError(msg);
          setLoading(false);
        });
    }
  };

  const filteredPending = pendingList.filter(pc => 
    pc.horseName.toLowerCase().includes(search.toLowerCase()) ||
    pc.tournamentName.toLowerCase().includes(search.toLowerCase()) ||
    pc.ownerName.toLowerCase().includes(search.toLowerCase())
  );

  const filteredAssigned = assignedList.filter(ae => 
    (ae.horseName || '').toLowerCase().includes(search.toLowerCase()) ||
    (ae.tournamentName || '').toLowerCase().includes(search.toLowerCase()) ||
    (ae.raceName || '').toLowerCase().includes(search.toLowerCase()) ||
    (ae.ownerName || '').toLowerCase().includes(search.toLowerCase())
  );

  const filteredHistory = historyList.filter(mr => 
    mr.horseName.toLowerCase().includes(search.toLowerCase()) ||
    mr.tournamentName.toLowerCase().includes(search.toLowerCase()) ||
    mr.checkedByName.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 relative min-w-0 overflow-y-auto">
        <PageAmbience accent="red" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">

          <PageHero
            title="Kiểm Tra Sức Khỏe Ngựa"
            subtitle="Cung cấp bệnh án, xét nghiệm doping và đánh giá điều kiện tham gia giải đấu"
            imageUrl="/images/hero-referee.jpg"
            imagePosition="right 52%"
          />

          {error && (
            <div className="p-4 bg-red-500/10 border border-red-500/20 text-red-400 rounded-lg text-sm">
              {error}
            </div>
          )}

          {success && (
            <div className="p-4 bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 rounded-lg text-sm">
              {success}
            </div>
          )}

          {/* Tabs & Search */}
          <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between border-b border-glass-border">
            <div className="flex items-center gap-1">
              <button 
                onClick={() => { setActiveTab('pending'); setSearch(''); }}
                className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-all ${activeTab === 'pending' ? 'text-gold border-gold' : 'text-muted border-transparent hover:text-white'}`}
              >
                Chờ kiểm tra ({pendingList.length})
              </button>
              <button 
                onClick={() => { setActiveTab('assigned'); setSearch(''); }}
                className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-all ${activeTab === 'assigned' ? 'text-gold border-gold' : 'text-muted border-transparent hover:text-white'}`}
              >
                Tái khám / Rút ngựa ({assignedList.length})
              </button>
              <button 
                onClick={() => { setActiveTab('history'); setSearch(''); }}
                className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-all ${activeTab === 'history' ? 'text-gold border-gold' : 'text-muted border-transparent hover:text-white'}`}
              >
                Lịch sử khám ({historyList.length})
              </button>
            </div>

            <div className="flex items-center gap-2 bg-white/[0.04] border border-glass-border rounded-lg px-3 py-2 w-full md:w-64 mb-3">
              <Search size={14} className="text-muted shrink-0" />
              <input 
                value={search} 
                onChange={e => setSearch(e.target.value)} 
                placeholder="Tìm ngựa, giải đấu, chủ sở hữu..." 
                className="bg-transparent text-sm text-white placeholder:text-muted/60 outline-none w-full" 
              />
            </div>
          </div>

          {/* Pending List Tab */}
          {activeTab === 'pending' && (
            loading ? (
              <div className="text-center py-12 text-muted">Đang tải danh sách chờ...</div>
            ) : filteredPending.length === 0 ? (
              <div className="glass-panel rounded-xl p-12 text-center text-muted">
                Không tìm thấy lượt đăng ký chờ khám sức khỏe nào.
              </div>
            ) : (
              <div className="glass-panel rounded-xl overflow-hidden">
                <div className="overflow-x-auto">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="border-b border-glass-border bg-white/[0.02] text-xs font-semibold text-muted uppercase tracking-wider">
                        <th className="px-6 py-4">Ngựa</th>
                        <th className="px-6 py-4">Giải đấu</th>
                        <th className="px-6 py-4">Chủ sở hữu</th>
                        <th className="px-6 py-4">Ngày đăng ký</th>
                        <th className="px-6 py-4 text-right">Hành động</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-glass-border/40 text-sm text-white">
                      {filteredPending.map(pc => (
                        <tr key={pc.registrationId} className="hover:bg-white/[0.01] transition-colors">
                          <td className="px-6 py-4 font-medium text-gold">{pc.horseName}</td>
                          <td className="px-6 py-4 text-muted">{pc.tournamentName}</td>
                          <td className="px-6 py-4 text-muted">{pc.ownerName}</td>
                          <td className="px-6 py-4 text-muted">{new Date(pc.registeredAt).toLocaleDateString('vi-VN')}</td>
                          <td className="px-6 py-4 text-right">
                            <button
                              onClick={() => handleOpenCreateModal(pc)}
                              className="bg-gold/10 hover:bg-gold/20 text-gold hover:text-white px-3 py-1.5 rounded-lg text-xs font-bold transition-all inline-flex items-center gap-1 border border-gold/30"
                            >
                              <Plus size={12} /> Khám sức khỏe
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )
          )}

          {/* Assigned Entries Tab */}
          {activeTab === 'assigned' && (
            loading ? (
              <div className="text-center py-12 text-muted">Đang tải danh sách ngựa đã xếp lịch đua...</div>
            ) : filteredAssigned.length === 0 ? (
              <div className="glass-panel rounded-xl p-12 text-center text-muted">
                Không tìm thấy lượt đăng ký ngựa nào đã xếp lịch đua.
              </div>
            ) : (
              <div className="glass-panel rounded-xl overflow-hidden">
                <div className="overflow-x-auto">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="border-b border-glass-border bg-white/[0.02] text-xs font-semibold text-muted uppercase tracking-wider">
                        <th className="px-6 py-4">Ngựa</th>
                        <th className="px-6 py-4">Giải đấu</th>
                        <th className="px-6 py-4">Lượt đua</th>
                        <th className="px-6 py-4">Jockey</th>
                        <th className="px-6 py-4">Lần khám trước</th>
                        <th className="px-6 py-4">Trạng thái đua</th>
                        <th className="px-6 py-4 text-right">Hành động</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-glass-border/40 text-sm text-white">
                      {filteredAssigned.map(ae => (
                        <tr key={ae.raceEntryId} className="hover:bg-white/[0.01] transition-colors">
                          <td className="px-6 py-4">
                            <div className="font-medium text-gold">{ae.horseName}</div>
                            <div className="text-xs text-muted">Chủ: {ae.ownerName}</div>
                          </td>
                          <td className="px-6 py-4 text-muted text-xs max-w-[150px] truncate">{ae.tournamentName}</td>
                          <td className="px-6 py-4">
                            <div className="text-white text-xs">{ae.raceName || 'Lượt đua'}</div>
                            <div className="text-[10px] text-muted">Làn: {ae.laneNo}</div>
                          </td>
                          <td className="px-6 py-4 text-muted text-xs">{ae.jockeyName || 'Chưa nhận'}</td>
                          <td className="px-6 py-4">
                            {ae.lastMedicalResult ? (
                              <div className="space-y-1">
                                <span className={`px-2 py-0.5 rounded text-[10px] font-semibold ${
                                  ae.lastMedicalResult === 'Pass' ? 'bg-emerald-500/10 text-emerald-400' : 'bg-red-500/10 text-red-400'
                                }`}>
                                  {ae.lastMedicalResult === 'Pass' ? 'Đạt' : 'Không đạt'}
                                </span>
                                <div className="text-[10px] text-muted mt-1">{ae.lastCheckType === 'ReCheck' ? 'Tái khám' : 'Ban đầu'}</div>
                              </div>
                            ) : (
                              <span className="text-muted text-xs">Chưa khám</span>
                            )}
                          </td>
                          <td className="px-6 py-4">
                            <span className="text-xs font-semibold px-2 py-0.5 rounded bg-blue-500/10 text-blue-400 border border-blue-500/20">
                              {ae.raceEntryStatus}
                            </span>
                          </td>
                          <td className="px-6 py-4 text-right">
                            <button
                              onClick={() => handleOpenRecheckModal(ae)}
                              className="bg-red-500/10 hover:bg-red-500/20 text-red-400 hover:text-white px-3 py-1.5 rounded-lg text-xs font-bold transition-all inline-flex items-center gap-1 border border-red-500/30"
                            >
                              <RefreshCw size={12} className="animate-spin-slow" /> Tái khám y tế
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )
          )}

          {/* History Tab */}
          {activeTab === 'history' && (
            loading ? (
              <div className="text-center py-12 text-muted">Đang tải lịch sử khám...</div>
            ) : filteredHistory.length === 0 ? (
              <div className="glass-panel rounded-xl p-12 text-center text-muted">
                Không tìm thấy bệnh án sức khỏe nào đã ghi nhận.
              </div>
            ) : (
              <div className="glass-panel rounded-xl overflow-hidden">
                <div className="overflow-x-auto">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="border-b border-glass-border bg-white/[0.02] text-xs font-semibold text-muted uppercase tracking-wider">
                        <th className="px-6 py-4">Ngựa</th>
                        <th className="px-6 py-4">Giải đấu / Kiểu khám</th>
                        <th className="px-6 py-4">Cân nặng</th>
                        <th className="px-6 py-4">Nhiệt độ</th>
                        <th className="px-6 py-4">Nhịp tim</th>
                        <th className="px-6 py-4">Doping</th>
                        <th className="px-6 py-4">Y tế / Lý do</th>
                        <th className="px-6 py-4">Người khám</th>
                        <th className="px-6 py-4 text-right">Hành động</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-glass-border/40 text-sm text-white">
                      {filteredHistory.map(mr => (
                        <tr key={mr.id} className="hover:bg-white/[0.01] transition-colors">
                          <td className="px-6 py-4 font-medium text-gold">{mr.horseName}</td>
                          <td className="px-6 py-4">
                            <div className="text-muted text-xs max-w-[150px] truncate">{mr.tournamentName}</div>
                            <span className={`inline-block mt-1 px-1.5 py-0.5 rounded text-[10px] font-medium ${
                              mr.checkType === 'ReCheck' ? 'bg-purple-500/20 text-purple-300 border border-purple-500/30' : 'bg-blue-500/20 text-blue-300 border border-blue-500/30'
                            }`}>
                              {mr.checkType === 'ReCheck' ? 'Tái khám' : 'Lần đầu'}
                            </span>
                          </td>
                          <td className="px-6 py-4 font-mono">{mr.weight} kg</td>
                          <td className="px-6 py-4 font-mono">{mr.temperature ? `${mr.temperature}°C` : '-'}</td>
                          <td className="px-6 py-4 font-mono">{mr.heartRate ? `${mr.heartRate} bpm` : '-'}</td>
                          <td className="px-6 py-4">
                            <span className={`px-2 py-0.5 rounded text-xs font-semibold ${
                              mr.dopingResult === 'Negative' ? 'bg-emerald-500/10 text-emerald-400' : 'bg-red-500/10 text-red-400'
                            }`}>
                              {mr.dopingResult === 'Negative' ? 'Âm tính' : 'Dương tính'}
                            </span>
                          </td>
                          <td className="px-6 py-4">
                            <div className="space-y-1">
                              <span className={`px-2 py-0.5 rounded text-xs font-semibold ${
                                mr.medicalResult === 'Pass' ? 'bg-emerald-500/10 text-emerald-400' : 'bg-red-500/10 text-red-400'
                              }`}>
                                {mr.medicalResult === 'Pass' ? 'Đạt' : 'Không đạt'}
                              </span>
                              {mr.failReason && (
                                <div className="text-xs text-red-400 max-w-[150px] truncate" title={mr.failReason}>
                                  Lý do: {mr.failReason === 'FailedMedicalReCheck' ? 'Tái khám không đạt' : 
                                           mr.failReason === 'VeterinaryDecision' ? 'Quyết định của Vet' : 
                                           mr.failReason === 'HorseInjury' ? 'Ngựa chấn thương' : mr.failReason}
                                </div>
                              )}
                            </div>
                          </td>
                          <td className="px-6 py-4 text-muted text-xs">{mr.checkedByName}</td>
                          <td className="px-6 py-4 text-right space-x-2 shrink-0">
                            <button
                              onClick={() => handleOpenEditModal(mr)}
                              className="text-gold hover:text-white bg-gold/10 hover:bg-gold/20 p-1.5 rounded transition-all inline-flex items-center"
                              title="Chỉnh sửa bệnh án"
                            >
                              <Edit2 size={12} />
                            </button>
                            {mr.checkType !== 'ReCheck' && (
                              <button
                                onClick={() => handleDeleteRecord(mr.id)}
                                className="text-red-400 hover:text-white bg-red-500/10 hover:bg-red-500/20 p-1.5 rounded transition-all inline-flex items-center"
                                title="Xóa bệnh án"
                              >
                                <Trash2 size={12} />
                              </button>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )
          )}

          {/* Form Modal */}
          {showModal && (
            <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
              <div className="bg-white border border-slate-200 rounded-xl w-full max-w-lg overflow-hidden shadow-2xl relative">
                <div className="px-6 py-4 border-b border-slate-100 flex justify-between items-center bg-slate-50">
                  <h3 className="font-serif text-lg font-bold text-slate-800">
                    {modalType === 'create' ? `Khám sức khỏe: ${selectedHorseName}` : 
                     modalType === 'recheck' ? `Tái khám y tế: ${selectedHorseName}` : 
                     `Chỉnh sửa bệnh án: ${selectedHorseName}`}
                  </h3>
                  <button 
                    onClick={() => setShowModal(false)}
                    className="text-slate-400 hover:text-slate-600 text-lg font-bold"
                  >
                    ×
                  </button>
                </div>
                
                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                  {modalType === 'recheck' && (
                    <div className="p-3 bg-amber-50 border border-amber-200 text-amber-800 rounded-lg text-xs">
                      ℹ️ Đợt tái khám này áp dụng cho ngựa đã được xếp lịch thi đấu. Nếu đánh giá không đạt (Fail), ngựa sẽ bị loại khỏi lượt đua.
                    </div>
                  )}

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-bold text-slate-600 uppercase mb-1">Cân nặng (kg) *</label>
                      <input
                        type="number"
                        step="0.01"
                        required
                        value={weight}
                        onChange={e => setWeight(e.target.value)}
                        placeholder="VD: 450.5"
                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 placeholder-slate-400 focus:border-gold focus:ring-1 focus:ring-gold outline-none transition-all"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-slate-600 uppercase mb-1">Nhiệt độ (°C)</label>
                      <input
                        type="number"
                        step="0.1"
                        value={temperature}
                        onChange={e => setTemperature(e.target.value)}
                        placeholder="VD: 38.2"
                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 placeholder-slate-400 focus:border-gold focus:ring-1 focus:ring-gold outline-none transition-all"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-xs font-bold text-slate-600 uppercase mb-1">Nhịp tim (bpm)</label>
                    <input
                      type="number"
                      value={heartRate}
                      onChange={e => setHeartRate(e.target.value)}
                      placeholder="VD: 40"
                      className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 placeholder-slate-400 focus:border-gold focus:ring-1 focus:ring-gold outline-none transition-all"
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-bold text-slate-600 uppercase mb-1">Kết quả Doping *</label>
                      <select
                        value={dopingResult}
                        onChange={e => setDopingResult(e.target.value)}
                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 focus:border-gold outline-none transition-all"
                      >
                        <option value="Negative" className="bg-white text-slate-800">Negative (Âm tính)</option>
                        <option value="Positive" className="bg-white text-slate-800">Positive (Dương tính)</option>
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-slate-600 uppercase mb-1">Đánh giá Y tế *</label>
                      <select
                        value={medicalResult}
                        onChange={e => setMedicalResult(e.target.value)}
                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 focus:border-gold outline-none transition-all"
                      >
                        <option value="Pass" className="bg-white text-slate-800">Pass (Đạt chuẩn)</option>
                        <option value="Fail" className="bg-white text-slate-800">Fail (Không đạt)</option>
                      </select>
                    </div>
                  </div>

                  {medicalResult === 'Fail' && (
                    <div className="animate-fade-in">
                      <label className="block text-xs font-bold text-red-600 uppercase mb-1">Lý do không đạt chuẩn *</label>
                      <select
                        value={failReason}
                        onChange={e => setFailReason(e.target.value)}
                        className="w-full bg-slate-50 border border-red-200 text-red-800 rounded-lg px-3 py-2 text-sm focus:border-red-500 outline-none transition-all"
                      >
                        <option value="FailedMedicalReCheck">Failed Medical Re-Check (Tái khám y khoa thất bại)</option>
                        <option value="VeterinaryDecision">Veterinary Decision (Quyết định của bác sĩ thú y)</option>
                        <option value="HorseInjury">Horse Injury (Ngựa gặp chấn thương)</option>
                        <option value="Scratch">Scratch (Bỏ cuộc / Rút trước vạch xuất phát)</option>
                      </select>
                    </div>
                  )}

                  <div>
                    <label className="block text-xs font-bold text-slate-600 uppercase mb-1">Ghi chú y khoa</label>
                    <textarea
                      value={notes}
                      onChange={e => setNotes(e.target.value)}
                      placeholder="Ghi chú thêm về sức khỏe ngựa..."
                      rows={3}
                      className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 placeholder-slate-400 focus:border-gold focus:ring-1 focus:ring-gold outline-none resize-none transition-all"
                    />
                  </div>

                  <div className="pt-4 flex justify-end gap-3 border-t border-slate-100">
                    <button
                      type="button"
                      onClick={() => setShowModal(false)}
                      className="px-4 py-2 border border-slate-200 hover:bg-slate-50 rounded-lg text-sm text-slate-600 hover:text-slate-800 transition-all"
                    >
                      Hủy
                    </button>
                    <button
                      type="submit"
                      disabled={loading}
                      className="bg-gold hover:bg-gold-hover text-black font-bold px-4 py-2 rounded-lg text-sm transition-all"
                    >
                      {loading ? 'Đang lưu...' : 'Lưu kết quả'}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          )}

        </main>
      </div>
    </div>
  );
}

