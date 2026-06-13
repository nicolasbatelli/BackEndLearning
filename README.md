# SocialChat

Full-stack multi-user chat platform.

## Structure

- `SocialChat.backend/` - .NET 8 API (DDD, CQRS, EF Core, SignalR)
- `SocialChat.frontEnd/` - Next.js + Material UI client
- `docker-compose.yml` - SQL Server for local development

## Prerequisites

- .NET 8 SDK
- Node.js 20+
- Docker Desktop
- MailerSend API token + verified sender (https://app.mailersend.com/)
- Google OAuth client ID (for Google sign-in)

Profile pictures are resized to a 128x128 thumbnail (via ImageSharp) and stored directly in
the database as `varbinary`. They are returned to the client as a base64 data URI, so no
external object storage is required.

## Backend setup

```bash
docker compose up -d
cd SocialChat.backend
dotnet user-secrets init --project SocialChat.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=SocialChatDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;" --project SocialChat.Api
dotnet user-secrets set "MailerSend:ApiToken" "YOUR_MAILERSEND_API_TOKEN" --project SocialChat.Api
dotnet user-secrets set "MailerSend:FromEmail" "noreply@yourdomain.com" --project SocialChat.Api
dotnet user-secrets set "GoogleAuth:ClientId" "YOUR_GOOGLE_CLIENT_ID" --project SocialChat.Api
dotnet run --project SocialChat.Api
```

API: http://localhost:5000  
Swagger: http://localhost:5000/swagger

## Frontend setup

```bash
cd SocialChat.frontEnd
cp .env.local.example .env.local
npm install
npm run dev
```

App: http://localhost:3000

## Tests

```bash
cd SocialChat.backend && dotnet test
cd SocialChat.frontEnd && npm test
```

---

# Guía de configuración (paso a paso)

Esta sección explica, en español, cómo dejar todo funcionando desde cero: base de datos, envío de correos (MailerSend), inicio de sesión con Google, backend y frontend.

## 0. Requisitos previos

Instalar antes de empezar:

- .NET 8 SDK
- Node.js 20 o superior
- Docker Desktop (corriendo)
- Una cuenta en MailerSend (https://app.mailersend.com/)
- Una cuenta de Google (para crear las credenciales de OAuth)

## 1. Levantar SQL Server con Docker

Desde la raíz del proyecto:

```bash
docker compose up -d
```

Esto crea un contenedor de SQL Server escuchando en `localhost:1433` con la contraseña definida en `docker-compose.yml`. Para verificar que está corriendo:

```bash
docker ps
```

## 2. Obtener el token de MailerSend (envío de correos)

El backend usa MailerSend para enviar el correo de verificación de cuenta.

1. Entrá a https://app.mailersend.com/ y creá una cuenta.
2. Verificá un dominio en **Domains** (o usá el dominio de prueba `trial` que MailerSend te da; ese solo permite enviar correos a tu propia dirección registrada).
3. Andá a **Integrations / API tokens** → **Generate new token**.
4. Copiá el token (empieza con `mlsn.`).
5. Anotá también el **From email** (debe pertenecer a un dominio verificado, por ejemplo `noreply@tudominio.com`).

> Nota: con el dominio de prueba (trial) solo vas a poder enviar el correo de verificación a la dirección con la que te registraste en MailerSend. Para enviar a cualquier usuario necesitás verificar un dominio propio.

## 3. Crear el Client ID de Google (inicio de sesión con Google)

1. Entrá a https://console.cloud.google.com/ e iniciá sesión.
2. Creá un proyecto (arriba a la izquierda → **Nuevo proyecto** → nombre `SocialChat`) y seleccionalo.
3. **Configurar la pantalla de consentimiento** (menú izquierdo → **Pantalla de consentimiento / OAuth consent screen**):
   - Tipo de usuario: **Externo** → **Crear**.
   - Nombre de la app: `SocialChat`, correo de asistencia y datos de contacto del desarrollador (tu email).
   - **Guardar y continuar** (podés saltear "Scopes").
   - En **Usuarios de prueba** agregá la cuenta de Gmail con la que vas a iniciar sesión. (Importante: en modo testing solo esas cuentas pueden ingresar.)
4. **Crear las credenciales** (menú izquierdo → **Credenciales**):
   - **+ Crear credenciales** → **ID de cliente de OAuth**.
   - Tipo de aplicación: **Aplicación web**.
   - Nombre: `SocialChat Web`.
   - **Orígenes autorizados de JavaScript** → agregar `http://localhost:3000`.
   - **URIs de redireccionamiento autorizados** → agregar `http://localhost:3000`.
   - **Crear**.
5. Copiá el **Client ID** (termina en `.apps.googleusercontent.com`).

> Errores comunes:
> - Si el botón de Google no aparece o da `idpiframe_initialization_failed`: el origen `http://localhost:3000` no está cargado exactamente (sin barra final) en "Orígenes autorizados de JavaScript".
> - Si el login dice "acceso bloqueado / app sin verificar": falta agregar tu cuenta en "Usuarios de prueba".
> - Los cambios pueden tardar unos minutos en aplicarse.

## 4. Configurar el backend (secrets)

Los datos sensibles NO se guardan en `appsettings.json`; se cargan con user-secrets:

```bash
cd SocialChat.backend
dotnet user-secrets init --project SocialChat.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=SocialChatDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;" --project SocialChat.Api
dotnet user-secrets set "MailerSend:ApiToken" "mlsn.TU_TOKEN_DE_MAILERSEND" --project SocialChat.Api
dotnet user-secrets set "MailerSend:FromEmail" "noreply@tudominio.com" --project SocialChat.Api
dotnet user-secrets set "GoogleAuth:ClientId" "TU_CLIENT_ID.apps.googleusercontent.com" --project SocialChat.Api
```

Luego ejecutá el API (esto aplica las migraciones y crea la base automáticamente):

```bash
dotnet run --project SocialChat.Api
```

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## 5. Configurar el frontend

```bash
cd SocialChat.frontEnd
cp .env.local.example .env.local
```

Editá `.env.local` y completá:

```
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_GOOGLE_CLIENT_ID=TU_CLIENT_ID.apps.googleusercontent.com
```

> El **Client ID de Google debe ser el mismo** en el backend (`GoogleAuth:ClientId`) y en el frontend (`NEXT_PUBLIC_GOOGLE_CLIENT_ID`).

Instalá dependencias y arrancá:

```bash
npm install
npm run dev
```

- App: http://localhost:3000

## 6. Probar de punta a punta

1. Abrí http://localhost:3000/sign-up y creá una cuenta.
2. Revisá tu correo (el que registraste en MailerSend si usás dominio de prueba) y hacé clic en el link de verificación.
3. Iniciá sesión en http://localhost:3000/sign-in (o probá el botón de Google).
4. Subí una foto de perfil: se guarda como miniatura 128x128 directamente en la base de datos.

> Recordá reiniciar el API y `npm run dev` después de cambiar secrets o `.env.local` para que tomen los valores nuevos.

## Features

- Standalone sign up with validation, email verification, profile picture upload
- Sign in with username/email + password
- Google sign-in
- JWT auth with refresh token cookie
- Real-time chat via SignalR
- Sidebar with favorites, groups, self-chat, and personal chats
- Friend search + invites
- Notification bell
