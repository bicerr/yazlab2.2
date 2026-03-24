import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '1m', target: 100 },
    { duration: '30s', target: 200 },
    { duration: '30s', target: 0 },
  ],
};

const BASE_URL = 'http://localhost:5100';
let token = '';

export function setup() {
  const loginRes = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
    email: 'test@test.com',
    password: '123456',
  }), { headers: { 'Content-Type': 'application/json' } });

  token = JSON.parse(loginRes.body).token;
  return { token };
}

export default function (data) {
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${data.token}`,
  };

  // Ürünleri listele
  const productsRes = http.get(`${BASE_URL}/products`, { headers });
  check(productsRes, {
    'products status 200': (r) => r.status === 200,
    'products response time < 500ms': (r) => r.timings.duration < 500,
  });

  // Sipariş oluştur
  const orderRes = http.post(`${BASE_URL}/orders`, JSON.stringify({
    productId: '000000000000000000000001',
    quantity: 1,
    customerEmail: 'test@test.com',
  }), { headers });
  check(orderRes, {
    'order status 201': (r) => r.status === 201,
  });

  sleep(1);
}