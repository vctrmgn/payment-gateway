docker stop testing_container || true
docker build -t magno-payment-gateway .
docker run --rm -p 8080:80 -d --name testing_container magno-payment-gateway