import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5001';

export const options = {
  scenarios: {
    normal_load: {
      executor: 'constant-vus',
      vus: 20,
      duration: '30s',
      startTime: '0s',
      tags: { scenario: 'normal' },
    },
    peak_load: {
      executor: 'constant-vus',
      vus: 50,
      duration: '30s',
      startTime: '35s',
      tags: { scenario: 'peak' },
    },
    stress_load: {
      executor: 'constant-vus',
      vus: 100,
      duration: '30s',
      startTime: '70s',
      tags: { scenario: 'stress' },
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<2000'],
    http_req_failed: ['rate<0.1'],
  },
};

function getAuthToken() {
  const loginPayload = JSON.stringify({
    username: 'testguest',
    password: 'testpass123',
  });

  const loginRes = http.post(`${BASE_URL}/api/v1/auth/login`, loginPayload, {
    headers: { 'Content-Type': 'application/json' },
  });

  if (loginRes.status === 200) {
    return JSON.parse(loginRes.body).token;
  }
  return null;
}

export function setup() {
  const users = [
    { username: 'testguest', password: 'testpass123', role: 'Guest' },
    { username: 'testhost', password: 'testpass123', role: 'Host' },
  ];

  users.forEach((user) => {
    http.post(`${BASE_URL}/api/v1/auth/register`, JSON.stringify(user), {
      headers: { 'Content-Type': 'application/json' },
    });
  });

  const hostLogin = http.post(
    `${BASE_URL}/api/v1/auth/login`,
    JSON.stringify({ username: 'testhost', password: 'testpass123' }),
    { headers: { 'Content-Type': 'application/json' } }
  );

  if (hostLogin.status === 200) {
    const hostToken = JSON.parse(hostLogin.body).token;
    for (let i = 0; i < 10; i++) {
      http.post(
        `${BASE_URL}/api/v1/listings`,
        JSON.stringify({
          noOfPeople: 4,
          country: 'Turkey',
          city: 'Istanbul',
          price: 100 + i * 10,
          title: `Test Listing ${i + 1}`,
          description: `A beautiful stay in Istanbul #${i + 1}`,
        }),
        {
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${hostToken}`,
          },
        }
      );
    }
  }

  const guestToken = getAuthToken();
  return { guestToken };
}

export default function (data) {
  const queryRes = http.get(
    `${BASE_URL}/api/v1/listings/search?DateFrom=2026-06-01&DateTo=2026-06-10&NoOfPeople=2&Country=Turkey&City=Istanbul&Page=1&PageSize=10`
  );

  check(queryRes, {
    'Query Listings - status is 200': (r) => r.status === 200,
    'Query Listings - has items': (r) => {
      try {
        const body = JSON.parse(r.body);
        return body.items !== undefined;
      } catch {
        return false;
      }
    },
  });

  sleep(0.5);

  if (data.guestToken) {
    const bookingPayload = JSON.stringify({
      listingId: 1,
      dateFrom: `2027-${String(Math.floor(Math.random() * 12) + 1).padStart(2, '0')}-01`,
      dateTo: `2027-${String(Math.floor(Math.random() * 12) + 1).padStart(2, '0')}-05`,
      namesOfPeople: ['Test User'],
    });

    const bookRes = http.post(`${BASE_URL}/api/v1/bookings`, bookingPayload, {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${data.guestToken}`,
      },
    });

    check(bookRes, {
      'Book a Stay - status is 200 or 400': (r) =>
        r.status === 200 || r.status === 400,
    });
  }

  sleep(0.5);
}
