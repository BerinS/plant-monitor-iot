# Build
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
# Copy the rest of the source code
COPY . .

RUN npm run build -- --configuration production

# Run 
FROM nginx:alpine

# CRITICAL FIX: Angular 17+ puts static files in /browser
# Replace 'plant-monitoring-ui' with your actual project name from angular.json
COPY --from=build /app/dist/plantmonitoring_frontend/browser /usr/share/nginx/html

# Copy your custom config
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]