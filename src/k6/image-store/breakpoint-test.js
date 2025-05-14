import { check, group } from "k6";
import http from "k6/http";

const fileData = open("Trollface.jpg", "b");

export const options = {
    executor: "ramping-arrival-rate", 
    stages: [
        { duration: "2h", target: 10000 },
    ],
    gracefulStop: "1m",

    thresholds: {
        "http_req_duration": [
            { threshold: "p(95)<800", abortOnFail: true, delayAbortEval: "20s" },
            { threshold: "avg < 500", abortOnFail: true, delayAbortVal: "20s" }
        ], 
        "http_req_failed": [{ threshold: "rate<0.01", abortOnFail: true, delayAbortVal: "20s" }],

        "checks": ["rate>0.95"], 
    },

};

export default function () {
    group("GetAllImages", () => {
        const response = http.get("http://localhost:8080/image-api/images");

        check(response, {
            "status is 200": (r) => r.status === 200,
        });
    });

    group("GetImageById", () => {
        const response = http.get("http://localhost:8080/image-api/images/353450e4-ed02-46f9-9ea7-f3ac18d938b5.jpg");
        check(response, {
            "status is 200": (r) => r.status === 200,
        });
    });

    if (__ITER % 10000 === 0) {
    group("PostImg", () => {
        const data = {
            file: http.file(fileData, "Trollface.jpg"),
        };

        const response = http.post("http://localhost:5000/image-api/images", data);
        check(response, {
            "status is 201": (r) => r.status === 201,
        });
   
    });
    };
}