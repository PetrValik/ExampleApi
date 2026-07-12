/**
 * Supported ISO 4217 alphabetic currency codes (49 codes).
 *
 * Currency is required and validated against this set only when `price > 0`;
 * it is ignored/optional for free articles (`price == 0`).
 */
export const SUPPORTED_CURRENCIES: string[] = [
  // Major currencies
  'USD', 'EUR', 'JPY', 'GBP', 'CHF', 'CAD', 'AUD', 'NZD',
  // European currencies
  'SEK', 'NOK', 'DKK', 'CZK', 'PLN', 'HUF', 'RON', 'BGN', 'ISK',
  // Asian currencies
  'CNY', 'HKD', 'SGD', 'KRW', 'INR', 'THB', 'MYR', 'IDR', 'PHP', 'TWD', 'VND',
  // Latin American currencies
  'BRL', 'MXN', 'ARS', 'CLP', 'COP', 'PEN',
  // Middle Eastern currencies
  'AED', 'SAR', 'ILS', 'QAR', 'KWD', 'BHD',
  // Other currencies
  'ZAR', 'TRY', 'RUB', 'UAH', 'EGP', 'NGN', 'KES', 'MAD', 'PKR',
];

const SUPPORTED_SET = new Set(SUPPORTED_CURRENCIES);

export function isSupportedCurrency(code: string | null | undefined): boolean {
  return typeof code === 'string' && SUPPORTED_SET.has(code);
}
