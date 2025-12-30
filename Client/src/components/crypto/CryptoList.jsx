import PropTypes from 'prop-types';

const CryptoList = ({ prices, onSymbolClick }) => {
  const formatPrice = price => {
    if (!price) return '0.00';
    const num = parseFloat(price);
    if (num < 0.01) {
      return num.toFixed(8);
    }
    return num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 8 });
  };

  return (
    <div
      style={{
        backgroundColor: '#1a1f3a',
        borderRadius: '12px',
        overflow: 'hidden',
      }}
    >
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ borderBottom: '1px solid #374151' }}>
            <th style={{ padding: '1rem', textAlign: 'left', color: '#9ca3af', fontWeight: '600' }}>
              Symbol
            </th>
            <th style={{ padding: '1rem', textAlign: 'right', color: '#9ca3af', fontWeight: '600' }}>
              Last Price
            </th>
          </tr>
        </thead>
        <tbody>
          {prices.map(priceData => {
            const lastPrice = priceData?.lastPrice || '0';

            return (
              <tr
                key={priceData.symbol}
                onClick={() => onSymbolClick(priceData.symbol)}
                style={{
                  borderBottom: '1px solid #374151',
                  cursor: 'pointer',
                  transition: 'background-color 0.2s',
                }}
                onMouseEnter={e => {
                  e.currentTarget.style.backgroundColor = '#0f172a';
                }}
                onMouseLeave={e => {
                  e.currentTarget.style.backgroundColor = 'transparent';
                }}
              >
                <td style={{ padding: '1rem', fontWeight: '600' }}>{priceData.symbol}</td>
                <td style={{ padding: '1rem', textAlign: 'right', fontFamily: 'monospace' }}>
                  {formatPrice(lastPrice)}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};

CryptoList.propTypes = {
  prices: PropTypes.arrayOf(
    PropTypes.shape({
      symbol: PropTypes.string.isRequired,
      lastPrice: PropTypes.string.isRequired,
    }),
  ).isRequired,
  onSymbolClick: PropTypes.func.isRequired,
};

export default CryptoList;
