import { Controller, Get } from '@nestjs/common';

/** Anonymous liveness probe. */
@Controller('health')
export class HealthController {
  @Get()
  getHealth(): { status: string } {
    return { status: 'healthy' };
  }
}
