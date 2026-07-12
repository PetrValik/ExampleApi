import { Injectable, UnauthorizedException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { JwtService } from '@nestjs/jwt';
import { TokenRequestDto } from './dto/token-request.dto';
import { TokenResponse } from './dto/token-response.dto';

/**
 * Demo authentication. The hardcoded `admin`/`admin` credentials stand in for a
 * real identity provider — replace {@link issueToken} before production.
 */
@Injectable()
export class AuthService {
  private static readonly DEMO_USERNAME = 'admin';
  private static readonly DEMO_PASSWORD = 'admin';

  constructor(
    private readonly jwtService: JwtService,
    private readonly configService: ConfigService,
  ) {}

  issueToken(request: TokenRequestDto): TokenResponse {
    if (
      request?.username !== AuthService.DEMO_USERNAME ||
      request?.password !== AuthService.DEMO_PASSWORD
    ) {
      throw new UnauthorizedException('Invalid credentials.');
    }

    const expirationMinutes = Number(
      this.configService.get<string>('JWT_EXPIRATION_MINUTES') ?? '60',
    );
    const expiresAt = new Date(Date.now() + expirationMinutes * 60_000);

    // The name claim, issuer, audience and expiry come from the JwtModule
    // configuration (see AuthModule).
    const token = this.jwtService.sign({
      name: request.username,
      sub: request.username,
    });

    return { token, expiresAt: expiresAt.toISOString() };
  }
}
