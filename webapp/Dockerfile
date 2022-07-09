FROM --platform=$BUILDPLATFORM node:16.15.1 as builder

# Note: This dockerfile is basically just for bringing it up in a stack.
# In reality, just serve the damn static files

WORKDIR /app

COPY package*.json ./
COPY tsconfig.json .

RUN npm ci

COPY src src
COPY public public

ARG APP_ENV=production
ENV APP_ENV ${APP_ENV}

ARG REACT_APP_API_ROOT=/api

RUN npm run build

FROM nginx:mainline-alpine as host

COPY nginx.conf /etc/nginx/nginx.conf

COPY --from=builder /app/build /usr/share/nginx/html

CMD ["nginx"]