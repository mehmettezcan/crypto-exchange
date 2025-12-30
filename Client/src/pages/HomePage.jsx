import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { cryptoAPI } from '../services/api';
import useCryptoWs from '../hooks/useCryptoWs';
import CryptoList from '../components/crypto/CryptoList';
import SymbolDetailModal from '../components/crypto/SymbolDetailModal';

const HomePage = () => {
  const { isAuthenticated } = useAuth();
  const [allPrices, setAllPrices] = useState([]);
  const [prices, setPrices] = useState([]);
  const [selectedSymbol, setSelectedSymbol] = useState(null);
  const [loading, setLoading] = useState(true);

  const wsUrl = import.meta.env.VITE_WS_URL || 'ws://localhost:5001/ws/crypto';
  const { prices: wsPrices, isConnected } = useCryptoWs(wsUrl);

  useEffect(() => {
    (async () => {
      try {
        const data = await cryptoAPI.getPrices();
        const normalizedAll = data.map(item => ({
          symbol: item.symbol,
          lastPrice: item.lastPrice,
          change24h: item.change24h,
          high24h: item.high24h,
          low24h: item.low24h,
          volume24h: item.volume24h,
        }));
        setAllPrices(normalizedAll);

        const normalized = data.map(item => ({
          symbol: item.symbol,
          lastPrice: item.lastPrice,
        }));
        setPrices(normalized);
      } catch (e) {
        // console.error('Piyasa bilgilerini alırken hata oluştu:', e);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  useEffect(() => {
    setPrices(prevPrices => {
      const priceMap = {};
      prevPrices.forEach(item => {
        priceMap[item.symbol] = {
          symbol: item.symbol,
          lastPrice: item.lastPrice,
        };
      });
      Object.values(wsPrices).forEach(wsItem => {
        priceMap[wsItem.symbol] = {
          symbol: wsItem.symbol,
          lastPrice: wsItem.lastPrice,
        };
      });
      return Object.values(priceMap);
    });

    setAllPrices(prevAllPrices => {
      const allPriceMap = {};
      prevAllPrices.forEach(item => {
        allPriceMap[item.symbol] = {
          symbol: item.symbol,
          lastPrice: item.lastPrice,
          change24h: item.change24h,
          high24h: item.high24h,
          low24h: item.low24h,
          volume24h: item.volume24h,
        };
      });
      Object.values(wsPrices).forEach(wsItem => {
        if (allPriceMap[wsItem.symbol]) {
          allPriceMap[wsItem.symbol] = {
            ...allPriceMap[wsItem.symbol],
            lastPrice: wsItem.lastPrice,
          };
        } else {
          allPriceMap[wsItem.symbol] = {
            symbol: wsItem.symbol,
            lastPrice: wsItem.lastPrice,
            change24h: wsItem.change24h || '0',
            high24h: wsItem.high24h || '0',
            low24h: wsItem.low24h || '0',
            volume24h: wsItem.volume24h || '0',
          };
        }
      });
      return Object.values(allPriceMap);
    });
  }, [wsPrices]);

  const handleSymbolClick = symbol => {
    if (!isAuthenticated) {
      window.location.href = '/login';
      return;
    }

    const s = typeof symbol === 'string' ? symbol : symbol?.symbol;
    setSelectedSymbol(s ?? null);
  };

  const logout = () => {
    localStorage.removeItem('token');
    window.location.reload();
  };

  const handleCloseModal = () => setSelectedSymbol(null);

  if (loading) {
    return (
      <div
        style={{
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '100vh',
          gap: '1.5rem',
        }}
      >
        <div
          style={{
            width: '50px',
            height: '50px',
            border: '4px solid rgba(59, 130, 246, 0.2)',
            borderTop: '4px solid #3b82f6',
            borderRadius: '50%',
            animation: 'spin 1s linear infinite',
          }}
        />
        <p
          style={{
            color: '#9ca3af',
            fontSize: '1rem',
            fontWeight: '500',
          }}
        >
          Kripto fiyatları yükleniyor...
        </p>
        <style>
          {`
            @keyframes spin {
              0% { transform: rotate(0deg); }
              100% { transform: rotate(360deg); }
            }
          `}
        </style>
      </div>
    );
  }

  return (
    <div style={{ padding: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <h2 style={{ margin: 0 }}>
          Crypto Markets
          <span style={{ marginLeft: '1rem', fontSize: '1rem' }}>
            {isConnected ? '🟢' : '🔴'}
          </span>
        </h2>
        <div>
          <button
            type="button"
            style={{ padding: '0.5rem 1rem', borderRadius: '6px', border: 'none', backgroundColor: '#3b82f6', color: '#ffffff', cursor: 'pointer' }}
            onClick={() => {
              if (isAuthenticated) {
                logout();
              } else {
                window.location.href = '/login';
              }
            }}
          >
            {isAuthenticated ? 'Logout' : 'Login'}
          </button>
        </div>
      </div>

      <CryptoList
        prices={prices ? Object.values(prices) : []}
        onSymbolClick={handleSymbolClick}
      />

      {selectedSymbol && (
        <SymbolDetailModal
          symbol={selectedSymbol}
          priceData={allPrices.find(item => item.symbol === selectedSymbol)}
          onClose={handleCloseModal}
        />
      )}
    </div>
  );
};

export default HomePage;
