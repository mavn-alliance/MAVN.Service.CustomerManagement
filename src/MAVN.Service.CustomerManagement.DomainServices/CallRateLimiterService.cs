using System;
using System.Threading.Tasks;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Services;
using StackExchange.Redis;

namespace MAVN.Service.CustomerManagement.DomainServices
{
    public class CallRateLimiterService : ICallRateLimiterService
    {
        private readonly CallRateLimitSettingsDto _settings;
        private readonly string _redisInstanceName;
        private const string EmailVerificationKeyPattern = "{0}::emailverification::{1}";
        private const string PhoneNumberVerificationKeyPattern = "{0}::phonenumberverification::{1}";

        private readonly IDatabase _db;

        public CallRateLimiterService(IConnectionMultiplexer connectionMultiplexer, CallRateLimitSettingsDto settings, string redisInstanceName)
        {
            _settings = settings;
            _redisInstanceName = redisInstanceName;
            _db = connectionMultiplexer.GetDatabase();
        }

        public Task ClearAllCallRecordsForEmailVerificationAsync(string customerId)
            => ClearAllCallRecordsAsync(customerId, EmailVerificationKeyPattern);

        public Task RecordEmailVerificationCallAsync(string customerId)
            => RecordCallAsync(customerId, EmailVerificationKeyPattern, _settings.EmailVerificationCallsMonitoredPeriod);

        public Task<bool> IsAllowedToCallEmailVerificationAsync(string customerId)
            => IsAllowedToCallAsync(customerId, EmailVerificationKeyPattern,
                _settings.EmailVerificationCallsMonitoredPeriod, _settings.EmailVerificationMaxAllowedRequestsNumber);

        public Task ClearAllCallRecordsForPhoneVerificationAsync(string customerId)
            => ClearAllCallRecordsAsync(customerId, PhoneNumberVerificationKeyPattern);

        public Task RecordPhoneVerificationCallAsync(string customerId)
            => RecordCallAsync(customerId, PhoneNumberVerificationKeyPattern, _settings.PhoneVerificationCallsMonitoredPeriod);

        public Task<bool> IsAllowedToCallPhoneVerificationAsync(string customerId)
            => IsAllowedToCallAsync(customerId, PhoneNumberVerificationKeyPattern,
                _settings.PhoneVerificationCallsMonitoredPeriod, _settings.PhoneVerificationMaxAllowedRequestsNumber);

        private async Task ClearAllCallRecordsAsync(string customerId, string pattern)
        {
            var key = GetKeyFromPattern(customerId, pattern);

            await _db.SortedSetRemoveRangeByScoreAsync(key, double.MinValue, double.MaxValue);
        }

        private async Task RecordCallAsync(string customerId, string pattern, TimeSpan monitoredPeriod)
        {
            var key = GetKeyFromPattern(customerId, pattern);

            await _db.SortedSetAddAsync(key, DateTime.UtcNow.ToString(), DateTime.UtcNow.Ticks);
            await _db.KeyExpireAsync(key, monitoredPeriod);
        }

        private async Task<bool> IsAllowedToCallAsync(string customerId, string pattern, TimeSpan monitoredPeriod, int maxNumberOfCalls)
        {
            await ClearOldCallRecordsAsync(customerId, pattern, monitoredPeriod);

            var key = GetKeyFromPattern(customerId, pattern);
            var now = DateTime.UtcNow;
            var activeCallRecords = await _db.SortedSetRangeByScoreAsync(key, (now - monitoredPeriod).Ticks,
                now.Ticks);

            return activeCallRecords.Length < maxNumberOfCalls;
        }

        private async Task ClearOldCallRecordsAsync(string customerId, string pattern, TimeSpan monitoredPeriod)
        {
            var key = GetKeyFromPattern(customerId, pattern);
            await _db.SortedSetRemoveRangeByScoreAsync(key, double.MinValue,
                (DateTime.UtcNow - monitoredPeriod).Ticks);
        }

        private string GetKeyFromPattern(string customerId, string pattern)
        {
            return string.Format(pattern, _redisInstanceName, customerId);
        }
    }
}
