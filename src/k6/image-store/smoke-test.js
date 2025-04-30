import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 4, 
  duration: '1m', 
};

export default () => {
    const res = http.get("http://localhost:8080/image-api/images/");

    check(res, {
        "status is 200": (r) => r.status === 200,
    });
  sleep(1);
  const res2 = http.get("http://localhost:8080/image-api/images/1.jpg");

  check(res2, {
      "status is 200": (r) => r.status === 200,
  });
};