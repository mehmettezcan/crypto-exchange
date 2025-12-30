import PropTypes from 'prop-types';

const SymbolDetailModal = ({ symbol, priceData, onClose }) => {
  const formatPrice = price => {
    if (!price) return '0.00';
    const num = parseFloat(price);
    if (num < 0.01) {
      return num.toFixed(8);
    }
    return num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 8 });
  };

  const formatVolume = volume => {
    if (!volume) return '0';
    const num = parseFloat(volume);
    if (num >= 1000000) {
      return `${(num / 1000000).toFixed(2)}M`;
    }
    if (num >= 1000) {
      return `${(num / 1000).toFixed(2)}K`;
    }
    return num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  };

  const getChangeColor = change => {
    if (!change) return '#9ca3af';
    const num = parseFloat(change);
    if (num > 0) return '#10b981';
    if (num < 0) return '#ef4444';
    return '#9ca3af';
  };

  const displayData = priceData;

  return (
    <div
      role="presentation"
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        zIndex: 1000,
        padding: '2rem',
      }}
      onClick={onClose}
      onKeyDown={e => {
        if (e.key === 'Escape') {
          onClose();
        }
      }}
    >
      <div
        role="dialog"
        aria-modal="true"
        style={{
          backgroundColor: '#1a1f3a',
          borderRadius: '12px',
          padding: '2rem',
          maxWidth: '600px',
          width: '100%',
          maxHeight: '90vh',
          overflow: 'auto',
          boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.5)',
        }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
          <h2 style={{ fontSize: '1.5rem', fontWeight: '700' }}>{symbol}</h2>
          <button
            type="button"
            onClick={onClose}
            style={{
              backgroundColor: 'transparent',
              border: 'none',
              color: '#9ca3af',
              fontSize: '1.5rem',
              cursor: 'pointer',
              padding: '0.5rem',
              lineHeight: 1,
            }}
          >
            ×
          </button>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          <div
            style={
              {
                display: 'grid',
                gridTemplateColumns: '1fr 1fr',
                gap: '1rem',
              }
            }
          >
            <div>
              <div style={{ color: '#9ca3af', marginBottom: '0.5rem', fontSize: '0.875rem' }}>
                Last Price
              </div>
              <div style={{ fontSize: '1.5rem', fontWeight: '700', fontFamily: 'monospace' }}>
                {formatPrice(displayData?.lastPrice || displayData?.lastPrice)}
              </div>
            </div>

            <div>
              <div style={{ color: '#9ca3af', marginBottom: '0.5rem', fontSize: '0.875rem' }}>
                24h Change
              </div>
              <div
                style={{
                  fontSize: '1.5rem',
                  fontWeight: '700',
                  fontFamily: 'monospace',
                  color: getChangeColor(displayData?.change24h || displayData?.change24h),
                }}
              >
                {parseFloat(displayData?.change24h || displayData?.change24h || 0) > 0 ? '+' : ''}
                {formatPrice(displayData?.change24h || displayData?.change24h)}
              </div>
            </div>

            <div>
              <div style={{ color: '#9ca3af', marginBottom: '0.5rem', fontSize: '0.875rem' }}>
                24h High
              </div>
              <div style={{ fontSize: '1.25rem', fontWeight: '600', fontFamily: 'monospace' }}>
                {formatPrice(displayData?.high24h || displayData?.high24h)}
              </div>
            </div>

            <div>
              <div style={{ color: '#9ca3af', marginBottom: '0.5rem', fontSize: '0.875rem' }}>
                24h Low
              </div>
              <div style={{ fontSize: '1.25rem', fontWeight: '600', fontFamily: 'monospace' }}>
                {formatPrice(displayData?.low24h || displayData?.low24h)}
              </div>
            </div>

            <div style={{ gridColumn: '1 / -1' }}>
              <div style={{ color: '#9ca3af', marginBottom: '0.5rem', fontSize: '0.875rem' }}>
                24h Volume
              </div>
              <div style={{ fontSize: '1.25rem', fontWeight: '600', fontFamily: 'monospace' }}>
                {formatVolume(displayData?.volume24h || displayData?.volume24h)}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

SymbolDetailModal.propTypes = {
  symbol: PropTypes.string.isRequired,
  priceData: PropTypes.shape({
    symbol: PropTypes.string,
    lastPrice: PropTypes.string,
    high24h: PropTypes.string,
    low24h: PropTypes.string,
    volume24h: PropTypes.string,
    change24h: PropTypes.string,
  }),
  onClose: PropTypes.func.isRequired,
};

SymbolDetailModal.defaultProps = {
  priceData: null,
};

export default SymbolDetailModal;
