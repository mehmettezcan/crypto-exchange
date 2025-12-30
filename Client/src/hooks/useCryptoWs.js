import { useEffect, useRef, useState } from 'react';

const toDto = raw => {
  const symbol = raw?.Symbol ?? raw?.symbol;
  if (!symbol) return null;

  return {
    symbol,
    lastPrice: raw?.LastPrice ?? raw?.lastPrice ?? '0',
    high24h: raw?.High24h ?? raw?.high24h ?? '0',
    low24h: raw?.Low24h ?? raw?.low24h ?? '0',
    volume24h: raw?.Volume24h ?? raw?.volume24h ?? '0',
    change24h: raw?.Change24h ?? raw?.change24h ?? '0',
    timestamp: raw?.Timestamp ?? raw?.timestamp ?? null,
  };
};

export default function useCryptoWs(wsUrl) {
  const wsRef = useRef(null);
  const [isConnected, setIsConnected] = useState(false);
  const [prices, setPrices] = useState({});

  useEffect(() => {
    let closedByUser = false;
    let reconnectTimer = null;

    const connect = () => {
      const ws = new WebSocket(wsUrl);
      wsRef.current = ws;

      ws.onopen = () => setIsConnected(true);

      ws.onclose = () => {
        setIsConnected(false);
        if (!closedByUser) reconnectTimer = setTimeout(connect, 1000);
      };

      ws.onmessage = e => {
        let msg;
        try {
          msg = JSON.parse(e.data);
        } catch {
          return;
        }

        if (msg?.type === 'priceUpdated' && msg?.data) {
          const dto = toDto(msg.data);
          if (!dto) return;

          setPrices(prev => ({
            ...prev,
            [dto.symbol]: dto,
          }));
          return;
        }

        if (msg?.type === 'snapshot' && Array.isArray(msg?.data)) {
          const map = {};
          msg.data.forEach(item => {
            const dto = toDto(item);
            if (dto) map[dto.symbol] = dto;
          });
          setPrices(map);
        }
      };
    };

    connect();

    return () => {
      closedByUser = true;
      if (reconnectTimer) clearTimeout(reconnectTimer);
      wsRef.current?.close();
    };
  }, [wsUrl]);

  return { prices, isConnected };
}
