//  <copyright file="StepTimer.hpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft</author>

#pragma once

#include <cstdint>

/**
 * \brief  Helper class for animation and simulation timing.
 */
class StepTimer
{
public:
    StepTimer() noexcept(false)
    {
        if (QueryPerformanceFrequency(&qpcFrequency) == FALSE) throw NativeException("Failed to query performance frequency.");

        if (QueryPerformanceCounter(&qpcLastTime) == FALSE) throw NativeException("Failed to query performance counter.");

        qpcMaxDelta = static_cast<uint64_t>(qpcFrequency.QuadPart / 10);
    }

    [[nodiscard]] uint64_t GetElapsedTicks() const noexcept { return elapsedTicks; }
    [[nodiscard]] double   GetElapsedSeconds() const noexcept { return TicksToSeconds(elapsedTicks); }

    [[nodiscard]] uint64_t GetTotalTicks() const noexcept { return totalTicks; }
    [[nodiscard]] double   GetTotalSeconds() const noexcept { return TicksToSeconds(totalTicks); }

    [[nodiscard]] uint32_t GetFrameCount() const noexcept { return frameCount; }
    [[nodiscard]] uint32_t GetFramesPerSecond() const noexcept { return framesPerSecond; }

    [[nodiscard]] uint64_t GetTargetElapsedTicks() const noexcept { return targetElapsedTicks; }
    [[nodiscard]] double   GetTargetElapsedSeconds() const noexcept { return TicksToSeconds(targetElapsedTicks); }

    [[nodiscard]] UINT GetTargetElapsedMilliseconds() const noexcept
    {
        return static_cast<UINT>(GetTargetElapsedSeconds() * 1000.0);
    }

    void SetFixedTimeStep(bool const isFixedTimestep) noexcept { isFixedTimeStep = isFixedTimestep; }

    void SetTargetElapsedTicks(uint64_t const targetElapsed) noexcept { targetElapsedTicks = targetElapsed; }

    void SetTargetElapsedSeconds(double const targetElapsed) noexcept
    {
        targetElapsedTicks = SecondsToTicks(targetElapsed);
    }

    static constexpr uint64_t TICKS_PER_SECOND = 10000000;

    static constexpr double TicksToSeconds(uint64_t const ticks) noexcept
    {
        return static_cast<double>(ticks) / TICKS_PER_SECOND;
    }

    static constexpr uint64_t SecondsToTicks(double const seconds) noexcept
    {
        return static_cast<uint64_t>(seconds * TICKS_PER_SECOND);
    }

    void ResetElapsedTime()
    {
        if (QueryPerformanceCounter(&qpcLastTime) == FALSE) throw NativeException("Failed to query performance counter.");

        leftOverTicks    = 0;
        framesPerSecond  = 0;
        framesThisSecond = 0;
        qpcSecondCounter = 0;
    }

    template <typename TUpdate>
    void Tick(TUpdate const& update)
    {
        LARGE_INTEGER currentTime;

        if (QueryPerformanceCounter(&currentTime) == FALSE) throw NativeException("Failed to query performance counter.");

        auto timeDelta = static_cast<uint64_t>(currentTime.QuadPart - qpcLastTime.QuadPart);

        qpcLastTime      = currentTime;
        qpcSecondCounter += timeDelta;

        if (timeDelta > qpcMaxDelta) timeDelta = qpcMaxDelta;

        timeDelta *= TICKS_PER_SECOND;
        timeDelta /= static_cast<uint64_t>(qpcFrequency.QuadPart);

        uint32_t const lastFrameCount = frameCount;

        if (isFixedTimeStep)
        {
            if (static_cast<uint64_t>(std::abs(static_cast<int64_t>(timeDelta - targetElapsedTicks))) < TICKS_PER_SECOND / 4000) timeDelta = targetElapsedTicks;

            leftOverTicks += timeDelta;

            while (leftOverTicks >= targetElapsedTicks)
            {
                elapsedTicks  = targetElapsedTicks;
                totalTicks    += targetElapsedTicks;
                leftOverTicks -= targetElapsedTicks;
                frameCount++;

                update();
            }
        }
        else
        {
            elapsedTicks  = timeDelta;
            totalTicks    += timeDelta;
            leftOverTicks = 0;
            frameCount++;

            update();
        }

        if (frameCount != lastFrameCount) framesThisSecond++;

        if (qpcSecondCounter >= static_cast<uint64_t>(qpcFrequency.QuadPart))
        {
            framesPerSecond  = framesThisSecond;
            framesThisSecond = 0;
            qpcSecondCounter %= static_cast<uint64_t>(qpcFrequency.QuadPart);
        }
    }

private:
    LARGE_INTEGER qpcFrequency{};
    LARGE_INTEGER qpcLastTime{};
    uint64_t      qpcMaxDelta;

    uint64_t elapsedTicks  = 0;
    uint64_t totalTicks    = 0;
    uint64_t leftOverTicks = 0;

    uint32_t frameCount       = 0;
    uint32_t framesPerSecond  = 0;
    uint32_t framesThisSecond = 0;
    uint64_t qpcSecondCounter = 0;

    bool     isFixedTimeStep    = false;
    uint64_t targetElapsedTicks = TICKS_PER_SECOND / 60;
};
