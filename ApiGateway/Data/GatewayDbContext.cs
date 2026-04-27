// ApiGateway does not use a database.
// It is a pure reverse-proxy powered by Ocelot that routes
// incoming client requests to the appropriate downstream microservice.
//
// Port map (all services):
//   ApiGateway          → http://localhost:5000
//   AuthService         → http://localhost:5100
//   PostService         → http://localhost:5200
//   CommentService      → http://localhost:5300
//   CategoryService     → http://localhost:5400
//   MediaService        → http://localhost:5500
//   NewsletterService   → http://localhost:5600
//   NotificationService → http://localhost:5700
